using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using StreamVideo.Core.Configs;
using StreamVideo.Core.InternalDTO.Events;
using StreamVideo.Core.InternalDTO.Models;
using StreamVideo.Core.Auth;
using StreamVideo.Core.Exceptions;
using StreamVideo.Core.InternalDTO.Requests;
using StreamVideo.Core.InternalDTO.Responses;
using StreamVideo.Core.LowLevelClient.Models;
using StreamVideo.Core.Web;
using StreamVideo.Libs;
using StreamVideo.Libs.AppInfo;
using StreamVideo.Libs.Auth;
using StreamVideo.Libs.Http;
using StreamVideo.Libs.Logs;
using StreamVideo.Libs.NetworkMonitors;
using StreamVideo.Libs.Serialization;
using StreamVideo.Libs.Time;
using StreamVideo.Libs.Utils;
using StreamVideo.Libs.Websockets;
#if STREAM_TESTS_ENABLED
using System.Runtime.CompilerServices;
#endif

#if STREAM_TESTS_ENABLED
[assembly: InternalsVisibleTo("StreamChat.Tests")] //StreamTodo: verify which Unity version introduced this
#endif

namespace StreamVideo.Core.LowLevelClient
{
    /// <summary>
    /// Stream Chat Client - maintains WebSockets connection, executes API calls and exposes Stream events to which you can subscribe.
    /// There should be only one instance of this client in your application.
    /// </summary>
    public class StreamVideoLowLevelClient : IStreamVideoLowLevelClient
    {
        public const string MenuPrefix = "Stream/";

        public static readonly Uri ServerBaseUrl = new Uri("wss://video.stream-io-api.com/video/connect");
        public static readonly Uri CoordinatorWebUri = new Uri("wss://chat.stream-io-api.com");

        public event ConnectionHandler Connected;
        public event Action Reconnecting;
        public event Action Disconnected;
        public event ConnectionStateChangeHandler ConnectionStateChanged;

        public event Action<string> EventReceived;


        // public IChannelApi ChannelApi { get; }
        // public IMessageApi MessageApi { get; }
        // public IModerationApi ModerationApi { get; }
        // public IUserApi UserApi { get; }
        // public IDeviceApi DeviceApi { get; }

        // [Obsolete(
        //     "This property presents only initial state of the LocalUser when connection is made and is not ever updated. " +
        //     "Please use the OwnUser object returned from StreamChatClient.Connected event. This property will  be removed in the future.")]
        // public OwnUser LocalUser { get; private set; }

        public ConnectionState ConnectionState
        {
            get => _connectionState;
            private set
            {
                if (_connectionState == value)
                {
                    return;
                }

                var previous = _connectionState;
                _connectionState = value;
                ConnectionStateChanged?.Invoke(previous, _connectionState);

                if (value == ConnectionState.Disconnected)
                {
                    Disconnected?.Invoke();
                }
            }
        }

        //StreamTodo: wrap all params in a ReconnectPolicy object
        public ReconnectStrategy ReconnectStrategy => _reconnectScheduler.ReconnectStrategy;
        public float ReconnectConstantInterval => _reconnectScheduler.ReconnectConstantInterval;
        public float ReconnectExponentialMinInterval => _reconnectScheduler.ReconnectExponentialMinInterval;
        public float ReconnectExponentialMaxInterval => _reconnectScheduler.ReconnectExponentialMaxInterval;
        public int ReconnectMaxInstantTrials => _reconnectScheduler.ReconnectMaxInstantTrials;
        public double? NextReconnectTime => _reconnectScheduler.NextReconnectTime;

        /// <summary>
        /// SDK Version number
        /// </summary>
        public static readonly Version SDKVersion = new Version(0, 1, 0);

        /// <summary>
        /// Use this method to create the main client instance or use StreamChatClient constructor to create a client instance with custom dependencies
        /// </summary>
        /// <param name="authCredentials">Authorization data with ApiKey, UserToken and UserId</param>
        public static IStreamVideoLowLevelClient CreateDefaultClient(AuthCredentials authCredentials,
            IStreamClientConfig config = default)
        {
            var factory = new StreamDependenciesFactory();
            
            config ??= StreamClientConfig.Default;
            var logs = factory.CreateLogger(config.LogLevel.ToLogLevel());
            var applicationInfo = factory.CreateApplicationInfo();
            var coordinatorWebSocket
                = factory.CreateWebsocketClient(logs, isDebugMode: config.LogLevel.IsDebugEnabled());
            var sfuWebSocket
                = factory.CreateWebsocketClient(logs, isDebugMode: config.LogLevel.IsDebugEnabled());
            var httpClient = factory.CreateHttpClient();
            var serializer = factory.CreateSerializer();
            var timeService = factory.CreateTimeService();
            var networkMonitor = factory.CreateNetworkMonitor();

            return new StreamVideoLowLevelClient(authCredentials, coordinatorWebSocket, sfuWebSocket, httpClient, serializer,
                timeService, networkMonitor, applicationInfo, logs, config);
        }

        /// <summary>
        /// Create Development Authorization Token. Dev tokens work only if you enable "Disable Auth Checks" in your project's Dashboard.
        /// Dev tokens bypasses authorization and should only be used during development and never in production!
        /// More info <see cref="https://getstream.io/chat/docs/unity/tokens_and_authentication/?language=unity#developer-tokens"/>
        /// </summary>
        public static string CreateDeveloperAuthToken(string userId)
        {
            if (!IsUserIdValid(userId))
            {
                throw new ArgumentException($"{nameof(userId)} can only contain: a-z, 0-9, @, _ and - ");
            }

            var header = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"; //  header content = {"alg": "HS256", "typ": "JWT"}
            var devSignature = "devToken";

            var payloadBytes = Encoding.UTF8.GetBytes("{\"user_id\":\"" + userId + "\"}");
            var payload = Base64UrlEncode(payloadBytes);
            return $"{header}.{payload}.{devSignature}";
        }

        /// <summary>
        /// Strip invalid characters from a given Stream user id. The only allowed characters are: a-z, 0-9, @, _ and -
        /// </summary>
        public static string SanitizeUserId(string userId)
        {
            if (IsUserIdValid(userId))
            {
                return userId;
            }

            return Regex.Replace(userId, @"[^\w\.@_-]", "", RegexOptions.None, TimeSpan.FromSeconds(1));
        }

        public StreamVideoLowLevelClient(AuthCredentials authCredentials, IWebsocketClient coordinatorWebSocket, IWebsocketClient sfuWebSocket,
            IHttpClient httpClient, ISerializer serializer, ITimeService timeService, INetworkMonitor networkMonitor,
            IApplicationInfo applicationInfo, ILogs logs, IStreamClientConfig config)
        {
            _authCredentials = authCredentials;
            _coordinatorWebSocket = coordinatorWebSocket ?? throw new ArgumentNullException(nameof(coordinatorWebSocket));
            _sfuWebSocket = sfuWebSocket ?? throw new ArgumentNullException(nameof(sfuWebSocket));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
            _networkMonitor = networkMonitor ?? throw new ArgumentNullException(nameof(networkMonitor));
            applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
            _logs = logs ?? throw new ArgumentNullException(nameof(logs));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _logs.Prefix = "[Stream Chat] ";

            _requestUriFactory = new RequestUriFactory(authProvider: this, connectionProvider: this, _serializer);

            _httpClient.AddDefaultCustomHeader("stream-auth-type", DefaultStreamAuthType);
            var header = BuildStreamClientHeader(applicationInfo);
            _httpClient.AddDefaultCustomHeader("X-Stream-Client", header);

            // StreamTODO: setup a wrapper around WS that handles health checks, error handling and reconnections
            _coordinatorWebSocket.ConnectionFailed += OnCoordinatorWebsocketsConnectionFailed;
            _coordinatorWebSocket.Disconnected += OnCoordinatorWebsocketDisconnected;
            
            _sfuWebSocket.ConnectionFailed += OnSfuWebsocketsConnectionFailed;
            _sfuWebSocket.Disconnected += OnSfuWebsocketDisconnected;

            // InternalChannelApi
            //     = new InternalChannelApi(httpClient, serializer, logs, _requestUriFactory, lowLevelClient: this);
            // InternalMessageApi
            //     = new InternalMessageApi(httpClient, serializer, logs, _requestUriFactory, lowLevelClient: this);
            // InternalModerationApi
            //     = new InternalModerationApi(httpClient, serializer, logs, _requestUriFactory, lowLevelClient: this);
            // InternalUserApi
            //     = new InternalUserApi(httpClient, serializer, logs, _requestUriFactory, lowLevelClient: this);
            // InternalDeviceApi
            //     = new InternalDeviceApi(httpClient, serializer, logs, _requestUriFactory, lowLevelClient: this);
            //
            // ChannelApi = new ChannelApi(InternalChannelApi);
            // MessageApi = new MessageApi(InternalMessageApi);
            // ModerationApi = new ModerationApi(InternalModerationApi);
            // UserApi = new UserApi(InternalUserApi);
            // DeviceApi = new DeviceApi(InternalDeviceApi);

            _reconnectScheduler = new ReconnectScheduler(_timeService, this, _networkMonitor);
            _reconnectScheduler.ReconnectionScheduled += OnReconnectionScheduled;

            RegisterEventHandlers();

            LogErrorIfUpdateIsNotBeingCalled();
        }

        public void ConnectUser(AuthCredentials userAuthCredentials)
        {
            SetConnectionCredentials(userAuthCredentials);
            Connect();
        }

        public void Connect()
        {
            SetConnectionCredentials(_authCredentials);

            if (!ConnectionState.IsValidToConnect())
            {
                throw new InvalidOperationException("Attempted to connect, but client is in state: " + ConnectionState);
            }

            TryCancelWaitingForUserConnection();

            //StreamTodo: hidden dependency on SetUser being called
            //StreamTodo: remove injected Func
            var coordinatorConnectUri = _requestUriFactory.CreateCoordinatorConnectionUri(() => BuildStreamClientHeader(new UnityApplicationInfo()));

            _logs.Info($"Attempt to connect to: {coordinatorConnectUri}");

            ConnectionState = ConnectionState.Connecting;

            // StreamTodo: move to better place
            _coordinatorWebSocket.ConnectAsync(coordinatorConnectUri).ContinueWith(t =>
            {
                _logs.Info("WS connected! Let's send the connect message");
                
                var wsAuthMsg = new WSAuthMessageRequest
                {
                    Token = ((IAuthProvider)this).UserToken,
                    UserDetails = new ConnectUserDetailsRequest
                    {
                        Id = ((IAuthProvider)this).UserId,
                        //Image = null,
                        //Name = null
                    }
                };

                var serializedAuthMsg = _serializer.Serialize(wsAuthMsg);
                
                _coordinatorWebSocket.Send(serializedAuthMsg);
                
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task DisconnectAsync(bool permanent = false)
        {
            TryCancelWaitingForUserConnection();
            //StreamTodo: remove this, this cannot be used when internal disconnect due to expired token. Perhaps we should allow user to Suspend() and Unsupend() the client reconnection

            if (permanent)
            {
                _reconnectScheduler.Stop();
            }

            await _coordinatorWebSocket.DisconnectAsync(WebSocketCloseStatus.NormalClosure, "User called Disconnect");
        }

        public void Update(float deltaTime)
        {
#if !STREAM_TESTS_ENABLED
            _updateCallReceived = true;
#endif

            TryHandleWebsocketsConnectionFailed();
            TryToReconnect();

            UpdateHealthCheck();

            _coordinatorWebSocket.Update();

            while (_coordinatorWebSocket.TryDequeueMessage(out var msg))
            {
#if STREAM_DEBUG_ENABLED
                _logs.Info("WS message: " + msg);
#endif
                HandleNewWebsocketMessage(msg);
            }
        }

        //StreamTodo: move this to injected config object
        public void SetReconnectStrategySettings(ReconnectStrategy reconnectStrategy, float? exponentialMinInterval,
            float? exponentialMaxInterval, float? constantInterval)
        {
            _reconnectScheduler.SetReconnectStrategySettings(reconnectStrategy, exponentialMinInterval, exponentialMaxInterval, constantInterval);
        }

        public void Dispose()
        {
            ConnectionState = ConnectionState.Closing;
            
            _reconnectScheduler.Dispose();

            TryCancelWaitingForUserConnection();

            _coordinatorWebSocket.ConnectionFailed -= OnCoordinatorWebsocketsConnectionFailed;
            _coordinatorWebSocket.Disconnected -= OnCoordinatorWebsocketDisconnected;
            _coordinatorWebSocket.Dispose();
            
            _sfuWebSocket.ConnectionFailed -= OnSfuWebsocketsConnectionFailed;
            _sfuWebSocket.Disconnected -= OnCoordinatorWebsocketDisconnected;
            _sfuWebSocket.Dispose();
            
            _updateMonitorCts.Cancel();
        }

        string IAuthProvider.ApiKey => _authCredentials.ApiKey;
        string IAuthProvider.UserToken => _authCredentials.UserToken;
        string IAuthProvider.UserId => _authCredentials.UserId;
        string IAuthProvider.StreamAuthType => DefaultStreamAuthType;
        string IConnectionProvider.ConnectionId => _connectionId;
        Uri IConnectionProvider.ServerUri => ServerBaseUrl;

        // internal IInternalChannelApi InternalChannelApi { get; }
        // internal IInternalMessageApi InternalMessageApi { get; }
        // internal IInternalModerationApi InternalModerationApi { get; }
        // internal InternalUserApi InternalUserApi { get; }
        // internal IInternalDeviceApi InternalDeviceApi { get; }

        internal async Task<OwnUserResponse> ConnectUserAsync(string apiKey, string userId,
            ITokenProvider tokenProvider, CancellationToken cancellationToken = default)
        {
            if (!ConnectionState.IsValidToConnect())
            {
                throw new InvalidOperationException("Attempted to connect, but client is in state: " + ConnectionState);
            }

            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            SetPartialConnectionCredentials(apiKey, userId);

            TryCancelWaitingForUserConnection();

            ConnectionState = ConnectionState.Connecting;

            _connectUserCancellationToken = cancellationToken;

            _connectUserCancellationTokenSource
                = CancellationTokenSource.CreateLinkedTokenSource(_connectUserCancellationToken);
            _connectUserCancellationTokenSource.Token.Register(TryCancelWaitingForUserConnection);

            _connectUserTaskSource = new TaskCompletionSource<OwnUserResponse>();

            try
            {
                await RefreshAuthTokenFromProvider();

                var connectionUri = _requestUriFactory.CreateCoordinatorConnectionUri(() => BuildStreamClientHeader(new UnityApplicationInfo()));

                await _coordinatorWebSocket.ConnectAsync(connectionUri);

                // StreamTODO: Do we receive a user here?
                var ownUserDto = await _connectUserTaskSource.Task;
                return ownUserDto;
            }
            catch (Exception e)
            {
                _logs.Exception(e);
                ConnectionState = ConnectionState.Disconnected;
                throw;
            }
        }

        private const string DefaultStreamAuthType = "jwt";
        private const int HealthCheckMaxWaitingTime = 30;

        // For WebGL there is a slight delay when sending therefore we send HC event a bit sooner just in case
        private const int HealthCheckSendInterval = HealthCheckMaxWaitingTime - 1;

        private readonly IWebsocketClient _coordinatorWebSocket;
        private readonly IWebsocketClient _sfuWebSocket;
        private readonly ISerializer _serializer;
        private readonly ILogs _logs;
        private readonly ITimeService _timeService;
        private readonly INetworkMonitor _networkMonitor;
        private readonly IRequestUriFactory _requestUriFactory;
        private readonly IHttpClient _httpClient;
        private readonly StringBuilder _errorSb = new StringBuilder();
        private readonly StringBuilder _logSb = new StringBuilder();
        private readonly IStreamClientConfig _config;
        private readonly ReconnectScheduler _reconnectScheduler;

        private readonly Dictionary<string, Action<string>> _eventKeyToHandler =
            new Dictionary<string, Action<string>>();

        private readonly object _websocketConnectionFailedFlagLock = new object();

        private TaskCompletionSource<OwnUserResponse> _connectUserTaskSource;
        private CancellationToken _connectUserCancellationToken;
        private CancellationTokenSource _connectUserCancellationTokenSource;
        private CancellationTokenSource _updateMonitorCts;

        private AuthCredentials _authCredentials;

        private ConnectionState _connectionState;
        private string _connectionId;
        private float _lastHealthCheckReceivedTime;
        private float _lastHealthCheckSendTime;
        private bool _updateCallReceived;

        private bool _websocketConnectionFailed;
        private ITokenProvider _tokenProvider;

        private async Task RefreshAuthTokenFromProvider()
        {
#if STREAM_DEBUG_ENABLED
            _logs.Info($"Request new auth token for user `{_authCredentials.UserId}`");
#endif
            try
            {
                var token = await _tokenProvider.GetTokenAsync(_authCredentials.UserId);
                _authCredentials = _authCredentials.CreateWithNewUserToken(token);
                SetConnectionCredentials(_authCredentials);

#if STREAM_DEBUG_ENABLED
                _logs.Info($"auth token received for user `{_authCredentials.UserId}`: " + token);
#endif
            }
            catch (Exception e)
            {
                throw new TokenProviderException(
                    $"Failed to get token from the {nameof(ITokenProvider)}. Inspect {nameof(e.InnerException)} for more information. ",
                    e);
            }
        }

        private void TryCancelWaitingForUserConnection()
        {
            if (_connectUserTaskSource == null)
            {
                return;
            }

            var isConnectTaskRunning = _connectUserTaskSource.Task != null && !_connectUserTaskSource.Task.IsCompleted;
            var isCancellationRequested = _connectUserCancellationTokenSource.IsCancellationRequested;

            if (isConnectTaskRunning && !isCancellationRequested)
            {
#if STREAM_DEBUG_ENABLED
                _logs.Info($"Try Cancel {_connectUserTaskSource}");
#endif
                _connectUserTaskSource.TrySetCanceled();
            }
        }

        // StreamTodo: refactor how connected state is determined
        private void OnCoordinatorWebsocketDisconnected()
        {
#if STREAM_DEBUG_ENABLED
            _logs.Warning("Coordinator Websocket Disconnected");
#endif
            //ConnectionState = ConnectionState.Disconnected;
        }
        
        private void OnSfuWebsocketDisconnected()
        {
#if STREAM_DEBUG_ENABLED
            _logs.Warning("Coordinator Websocket Disconnected");
#endif
            //ConnectionState = ConnectionState.Disconnected;
        }

        /// <summary>
        /// This event can be called by a background thread and we must propagate it on the main thread
        /// Otherwise any call to Unity API would result in Exception. Unity API can only be called from the main thread
        /// </summary>
        private void OnCoordinatorWebsocketsConnectionFailed()
        {
            lock (_websocketConnectionFailedFlagLock)
            {
                _websocketConnectionFailed = true;
            }
        }
        
        /// <summary>
        /// This event can be called by a background thread and we must propagate it on the main thread
        /// Otherwise any call to Unity API would result in Exception. Unity API can only be called from the main thread
        /// </summary>
        private void OnSfuWebsocketsConnectionFailed()
        {
            lock (_websocketConnectionFailedFlagLock)
            {
                _websocketConnectionFailed = true;
            }
        }

        private void TryHandleWebsocketsConnectionFailed()
        {
            lock (_websocketConnectionFailedFlagLock)
            {
                if (!_websocketConnectionFailed)
                {
                    return;
                }

                _websocketConnectionFailed = false;
            }

#if STREAM_DEBUG_ENABLED
            _logs.Warning("Websocket connection failed");
#endif

            ConnectionState = ConnectionState.Disconnected;
        }
        
        /// <summary>
        /// Based on receiving initial health check event from the server
        /// </summary>
        private void OnConnectionConfirmed(HealthCheckEvent healthCheckEvent)
        {
            //StreamTodo: resolve issue that expired token also triggers connection confirmed that gets immediately disconnected

            _connectionId = healthCheckEvent.ConnectionId;
#pragma warning disable 0618
            //LocalUser = connectedEvent.Me;
#pragma warning restore 0618
            _lastHealthCheckReceivedTime = _timeService.Time;
            
            ConnectionState = ConnectionState.Connected;

            //_connectUserTaskSource?.SetResult(healthCheckEvent.Me);

            _logs.Info("Connection confirmed by server with connection id: " + _connectionId);
            //Connected?.Invoke(connectedEvent.Me);
            //InternalConnected?.Invoke(eventHealthCheckInternalDto);
        }

        /// <summary>
        /// Based on receiving initial health check event from the server
        /// </summary>
        private void OnConnectionConfirmed(ConnectedEvent connectedEvent)
        {
            //StreamTodo: resolve issue that expired token also triggers connection confirmed that gets immediately disconnected

            _connectionId = connectedEvent.ConnectionId;
#pragma warning disable 0618
            //LocalUser = connectedEvent.Me;
#pragma warning restore 0618
            _lastHealthCheckReceivedTime = _timeService.Time;
            
            ConnectionState = ConnectionState.Connected;

            _connectUserTaskSource?.SetResult(connectedEvent.Me);

            _logs.Info("Connection confirmed by server with connection id: " + _connectionId);
            //Connected?.Invoke(connectedEvent.Me);
            //InternalConnected?.Invoke(eventHealthCheckInternalDto);
        }

        private void TryToReconnect()
        {
            if (!ConnectionState.IsValidToConnect() || !NextReconnectTime.HasValue)
            {
                return;
            }

            if (NextReconnectTime.Value > _timeService.Time)
            {
                return;
            }

            Reconnecting?.Invoke();

            if (_tokenProvider != null)
            {
                ConnectUserAsync(_authCredentials.ApiKey, _authCredentials.UserId, _tokenProvider).LogIfFailed();
            }
            else
            {
                Connect();
            }
        }

        private void RegisterEventHandlers()
        {
            // Handle ConnectedEvent with the OwnUserResponse
            
            
            RegisterEventType<HealthCheckEvent>(WSEventType.HealthCheck,
                HandleHealthCheckEvent);
            //
            // RegisterEventType<MessageNewEventInternalDTO, EventMessageNew>(WSEventType.MessageNew,
            //     (e, dto) => MessageReceived?.Invoke(e), dto => InternalMessageReceived?.Invoke(dto));
            // RegisterEventType<MessageDeletedEventInternalDTO, EventMessageDeleted>(WSEventType.MessageDeleted,
            //     (e, dto) => MessageDeleted?.Invoke(e), dto => InternalMessageDeleted?.Invoke(dto));
            // RegisterEventType<MessageUpdatedEventInternalDTO, EventMessageUpdated>(WSEventType.MessageUpdated,
            //     (e, dto) => MessageUpdated?.Invoke(e), dto => InternalMessageUpdated?.Invoke(dto));
            // RegisterEventType<MessageReadEventInternalDTO, EventMessageRead>(WSEventType.MessageRead,
            //     (e, dto) => MessageRead?.Invoke(e), dto => InternalMessageRead?.Invoke(dto));
            //
            // RegisterEventType<ChannelUpdatedEventInternalDTO, EventChannelUpdated>(WSEventType.ChannelUpdated,
            //     (e, dto) => ChannelUpdated?.Invoke(e), dto => InternalChannelUpdated?.Invoke(dto));
            // RegisterEventType<ChannelDeletedEventInternalDTO, EventChannelDeleted>(WSEventType.ChannelDeleted,
            //     (e, dto) => ChannelDeleted?.Invoke(e), dto => InternalChannelDeleted?.Invoke(dto));
            // RegisterEventType<ChannelTruncatedEventInternalDTO, EventChannelTruncated>(WSEventType.ChannelTruncated,
            //     (e, dto) => ChannelTruncated?.Invoke(e), dto => InternalChannelTruncated?.Invoke(dto));
            // RegisterEventType<ChannelVisibleEventInternalDTO, EventChannelVisible>(WSEventType.ChannelVisible,
            //     (e, dto) => ChannelVisible?.Invoke(e), dto => InternalChannelVisible?.Invoke(dto));
            // RegisterEventType<ChannelHiddenEventInternalDTO, EventChannelHidden>(WSEventType.ChannelHidden,
            //     (e, dto) => ChannelHidden?.Invoke(e), dto => InternalChannelHidden?.Invoke(dto));
            //
            // RegisterEventType<ReactionNewEventInternalDTO, EventReactionNew>(WSEventType.ReactionNew,
            //     (e, dto) => ReactionReceived?.Invoke(e), dto => InternalReactionReceived?.Invoke(dto));
            // RegisterEventType<ReactionUpdatedEventInternalDTO, EventReactionUpdated>(WSEventType.ReactionUpdated,
            //     (e, dto) => ReactionUpdated?.Invoke(e), dto => InternalReactionUpdated?.Invoke(dto));
            // RegisterEventType<ReactionDeletedEventInternalDTO, EventReactionDeleted>(WSEventType.ReactionDeleted,
            //     (e, dto) => ReactionDeleted?.Invoke(e), dto => InternalReactionDeleted?.Invoke(dto));
            //
            // RegisterEventType<MemberAddedEventInternalDTO, EventMemberAdded>(WSEventType.MemberAdded,
            //     (e, dto) => MemberAdded?.Invoke(e), dto => InternalMemberAdded?.Invoke(dto));
            // RegisterEventType<MemberRemovedEventInternalDTO, EventMemberRemoved>(WSEventType.MemberRemoved,
            //     (e, dto) => MemberRemoved?.Invoke(e), dto => InternalMemberRemoved?.Invoke(dto));
            // RegisterEventType<MemberUpdatedEventInternalDTO, EventMemberUpdated>(WSEventType.MemberUpdated,
            //     (e, dto) => MemberUpdated?.Invoke(e), dto => InternalMemberUpdated?.Invoke(dto));
            //
            // RegisterEventType<UserPresenceChangedEventInternalDTO, EventUserPresenceChanged>(
            //     WSEventType.UserPresenceChanged,
            //     (e, dto) => UserPresenceChanged?.Invoke(e), dto => InternalUserPresenceChanged?.Invoke(dto));
            // RegisterEventType<UserUpdatedEventInternalDTO, EventUserUpdated>(WSEventType.UserUpdated,
            //     (e, dto) => UserUpdated?.Invoke(e), dto => InternalUserUpdated?.Invoke(dto));
            // RegisterEventType<UserDeletedEventInternalDTO, EventUserDeleted>(WSEventType.UserDeleted,
            //     (e, dto) => UserDeleted?.Invoke(e), dto => InternalUserDeleted?.Invoke(dto));
            // RegisterEventType<UserBannedEventInternalDTO, EventUserBanned>(WSEventType.UserBanned,
            //     (e, dto) => UserBanned?.Invoke(e), dto => InternalUserBanned?.Invoke(dto));
            // RegisterEventType<UserUnbannedEventInternalDTO, EventUserUnbanned>(WSEventType.UserUnbanned,
            //     (e, dto) => UserUnbanned?.Invoke(e), dto => InternalUserUnbanned?.Invoke(dto));
            //
            // RegisterEventType<UserWatchingStartEventInternalDTO, EventUserWatchingStart>(WSEventType.UserWatchingStart,
            //     (e, dto) => UserWatchingStart?.Invoke(e), dto => InternalUserWatchingStart?.Invoke(dto));
            // RegisterEventType<UserWatchingStopEventInternalDTO, EventUserWatchingStop>(WSEventType.UserWatchingStop,
            //     (e, dto) => UserWatchingStop?.Invoke(e), dto => InternalUserWatchingStop?.Invoke(dto));
            //
            // RegisterEventType<TypingStartEventInternalDTO, EventTypingStart>(WSEventType.TypingStart,
            //     (e, dto) => TypingStarted?.Invoke(e), dto => InternalTypingStarted?.Invoke(dto));
            // RegisterEventType<TypingStopEventInternalDTO, EventTypingStop>(WSEventType.TypingStop,
            //     (e, dto) => TypingStopped?.Invoke(e), dto => InternalTypingStopped?.Invoke(dto));

        }

        private void RegisterEventType<TDto, TEvent>(string key,
            Action<TEvent, TDto> handler, Action<TDto> internalHandler = null)
            where TEvent : ILoadableFrom<TDto, TEvent>, new()
        {
            if (_eventKeyToHandler.ContainsKey(key))
            {
                _logs.Warning($"Event handler with key `{key}` is already registered. Ignored");
                return;
            }

            _eventKeyToHandler.Add(key, serializedContent =>
            {
                try
                {
                    var eventObj = DeserializeEvent<TDto, TEvent>(serializedContent, out var dto);
                    handler?.Invoke(eventObj, dto);
                    internalHandler?.Invoke(dto);
                }
                catch (Exception e)
                {
                    _logs.Exception(e);
                }
            });
        }
        
        private void RegisterEventType<TDto>(string key,
            Action<TDto> internalHandler = null)
        {
            if (_eventKeyToHandler.ContainsKey(key))
            {
                _logs.Warning($"Event handler with key `{key}` is already registered. Ignored");
                return;
            }

            _eventKeyToHandler.Add(key, serializedContent =>
            {
                try
                {
                    var dto = _serializer.Deserialize<TDto>(serializedContent);
                    internalHandler?.Invoke(dto);
                }
                catch (Exception e)
                {
                    _logs.Exception(e);
                }
            });
        }

        private TEvent DeserializeEvent<TDto, TEvent>(string content, out TDto dto)
            where TEvent : ILoadableFrom<TDto, TEvent>, new()
        {
            try
            {
                dto = _serializer.Deserialize<TDto>(content);
            }
            catch (Exception e)
            {
                throw new StreamDeserializationException(content, typeof(TDto), e);
            }

            var response = new TEvent();
            response.LoadFromDto(dto);

            return response;
        }

        private void HandleNewWebsocketMessage(string msg)
        {
            const string ErrorKey = "error";

            if (_serializer.TryPeekValue<APIError>(msg, ErrorKey, out var apiError))
            {
                _errorSb.Length = 0;
                apiError.AppendFullLog(_errorSb);

                _logs.Error($"{nameof(APIError)} returned: {_errorSb}");
                return;
            }

            const string TypeKey = "type";

            if (!_serializer.TryPeekValue<string>(msg, TypeKey, out var type))
            {
                _logs.Error($"Failed to find `{TypeKey}` in msg: " + msg);
                return;
            }

            var time = DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss");
            EventReceived?.Invoke($"{time} - Event received: <b>{type}</b>");

            if (!_eventKeyToHandler.TryGetValue(type, out var handler))
            {
                if (_config.LogLevel.IsDebugEnabled())
                {
                    _logs.Warning($"No message handler registered for `{type}`. Message not handled: " + msg);
                }

                return;
            }

            handler(msg);
        }

        private void UpdateHealthCheck()
        {
            if (ConnectionState != ConnectionState.Connected)
            {
                return;
            }

            var timeSinceLastHealthCheckSent = _timeService.Time - _lastHealthCheckSendTime;
            if (timeSinceLastHealthCheckSent > HealthCheckSendInterval)
            {
                PingHealthCheck();
            }

            var timeSinceLastHealthCheck = _timeService.Time - _lastHealthCheckReceivedTime;
            if (timeSinceLastHealthCheck > HealthCheckMaxWaitingTime)
            {
                _logs.Warning($"Health check was not received since: {timeSinceLastHealthCheck}, reset connection");
                _coordinatorWebSocket
                    .DisconnectAsync(WebSocketCloseStatus.InternalServerError,
                        $"Health check was not received since: {timeSinceLastHealthCheck}")
                    .ContinueWith(_ => _logs.Exception(_.Exception), TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private void PingHealthCheck()
        {
            var healthCheck = new HealthCheckEvent
            {
                // StreamTodo: do we need to provide anything? Like connection_id? 
            };

            _coordinatorWebSocket.Send(_serializer.Serialize(healthCheck));
            _lastHealthCheckSendTime = _timeService.Time;

#if STREAM_DEBUG_ENABLED
            _logs.Info("Health check sent");
#endif
        }

        private void HandleHealthCheckEvent(HealthCheckEvent healthCheckEvent)
        {
            _lastHealthCheckReceivedTime = _timeService.Time;
        
            if (ConnectionState == ConnectionState.Connecting)
            {
                OnConnectionConfirmed(healthCheckEvent);
            }
        }

        private static bool IsUserIdValid(string userId)
        {
            var r = new Regex("^[a-zA-Z0-9@_-]+$");
            return r.IsMatch(userId);
        }

        private static string Base64UrlEncode(byte[] input)
            => Convert.ToBase64String(input)
                .Replace('+', '-')
                .Replace('/', '_')
                .Trim('=');

        private void SetConnectionCredentials(AuthCredentials credentials)
        {
            if (credentials.IsAnyEmpty())
            {
                throw new StreamMissingAuthCredentialsException(
                    "Please provide valid credentials: `Api Key`, 'User id`, `User token`");
            }

            _authCredentials = credentials;
            _httpClient.SetDefaultAuthenticationHeader(credentials.UserToken);
        }

        //StreamTodo: make it more clear that we either receive full set of credentials or apiKey, userId and the token provider
        private void SetPartialConnectionCredentials(string apiKey, string userId)
        {
            _authCredentials = new AuthCredentials(apiKey, userId, string.Empty);
        }

        private void LogErrorIfUpdateIsNotBeingCalled()
        {
            _updateMonitorCts = new CancellationTokenSource();
            
            //StreamTodo: temporarily disable update monitor when tests are enabled -> investigate why some tests trigger this error
#if !STREAM_TESTS_ENABLED
            const int timeout = 2;
            Task.Delay(timeout * 1000, _updateMonitorCts.Token).ContinueWith(t =>
            {
                if (!_updateCallReceived && !_updateMonitorCts.IsCancellationRequested && ConnectionState != ConnectionState.Closing)
                {
                    _logs.Error(
                        $"Connection is not being updated. Please call the `{nameof(StreamVideoLowLevelClient)}.{nameof(Update)}` method per frame. Connection state: {ConnectionState}");
                }
            }, _updateMonitorCts.Token);
#endif
        }

        private static string BuildStreamClientHeader(IApplicationInfo applicationInfo)
        {
            var sb = new StringBuilder();
            sb.Append($"stream-video-unity-client-");
            sb.Append(SDKVersion);
            // sb.Append("|");
            //
            // sb.Append("os=");
            // sb.Append(applicationInfo.OperatingSystem);
            // sb.Append("|");
            //
            // sb.Append("platform=");
            // sb.Append(applicationInfo.Platform);
            // sb.Append("|");
            //
            // sb.Append("engine=");
            // sb.Append(applicationInfo.Engine);
            // sb.Append("|");
            //
            // sb.Append("engine_version=");
            // sb.Append(applicationInfo.EngineVersion);
            // sb.Append("|");
            //
            // sb.Append("screen_size=");
            // sb.Append(applicationInfo.ScreenSize);
            // sb.Append("|");
            //
            // sb.Append("memory_size=");
            // sb.Append(applicationInfo.MemorySize);
            // sb.Append("|");
            //
            // sb.Append("graphics_memory_size=");
            // sb.Append(applicationInfo.GraphicsMemorySize);

            return sb.ToString();
        }
        
        private void OnReconnectionScheduled()
        {
            ConnectionState = ConnectionState.WaitToReconnect;
            var timeLeft = NextReconnectTime.Value - _timeService.Time;

            _logSb.Append("Reconnect scheduled to time: <b>");
            _logSb.Append(Math.Round(NextReconnectTime.Value));
            _logSb.Append(" seconds</b>, current time: <b>");
            _logSb.Append(Math.Round(_timeService.Time));
            _logSb.Append(" seconds</b>, time left: <b>");
            _logSb.Append(Math.Round(timeLeft));
            _logSb.Append(" seconds</b>");

            _logs.Info(_logSb.ToString());
            _logSb.Clear();
        }
    }
}
