// <auto-generated>
//     Generated by protoc-gen-twirpcs.  DO NOT EDIT!
// </auto-generated>
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Protobuf; // For ".ToByteArray()"
using Signal = StreamVideo.v1.Sfu.Signal;

namespace StreamVideo.Core.Sfu
{

    internal class GeneratedAPI
    {
        public static readonly MediaTypeWithQualityHeaderValue CONTENT_TYPE_PROTOBUF = new MediaTypeWithQualityHeaderValue("application/protobuf");

        public enum ErrorCode
        {
            NoError, Canceled, Unknown, Invalid_Argument, Malformed, Deadline_Exceeded, Not_Found, Bad_Route, Already_Exists, Permission_Denied,
            Unauthenticated, Resource_Exhausted, Failed_Precondition, Aborted, Out_Of_Range, Unimplemented, Internal, Unavailable, Data_Loss
        };

        public class Exception : System.Exception
        {
            public readonly ErrorCode Type;

            public Exception(ErrorCode type, string message) : base(message) { Type = type; }
        }

        private static GeneratedAPI.Exception createException(string jsonData)
        {
            var codeStr = parseJSONString(jsonData, "code");
            if (codeStr == null)
            {
                return new GeneratedAPI.Exception(ErrorCode.Unknown, jsonData);
            }

            ErrorCode errorCode = ErrorCode.Unknown;
            System.Enum.TryParse<ErrorCode>(codeStr, true, out errorCode);
            var msg = parseJSONString(jsonData, "msg");
            if (msg == null)
            {
                msg = jsonData;
            }
            return new GeneratedAPI.Exception(errorCode, msg);
        }

        private static string parseJSONString(string jsonData, string key)
        {
            var keyIndex = jsonData.IndexOf(key + "\":\"");
            if (keyIndex == -1) { return null; }
            keyIndex += key.Length + 3;
            var dataEnd = jsonData.IndexOf("\"", keyIndex);
            if (dataEnd == -1) { return null; }
            return jsonData.Substring(keyIndex, dataEnd - keyIndex);
        }

        private delegate Resp doParsing<Resp>(byte[] data) where Resp : IMessage;
        private static async Task<Resp> DoRequest<Req, Resp>(HttpClient client, string address, Req req, doParsing<Resp> parserFunc) where Req : IMessage where Resp : IMessage
        {
            using (var content = new ByteArrayContent(req.ToByteArray()))
            {
                content.Headers.ContentType = CONTENT_TYPE_PROTOBUF;
                using (HttpResponseMessage response = await client.PostAsync(address, content))
                {
                    var byteArr = await response.Content.ReadAsByteArrayAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorJSON = System.Text.Encoding.UTF8.GetString(byteArr, 0, byteArr.Length);
                        throw createException(errorJSON);
                    }
                    return parserFunc(byteArr);
                }
            }
        }

        // SetPublisher sends the WebRTC offer for the peer connection used to publish A/V
        public static async Task<Signal.SetPublisherResponse> SetPublisher(HttpClient client, Signal.SetPublisherRequest req)
        {
            return await DoRequest<Signal.SetPublisherRequest, Signal.SetPublisherResponse>(client, "/twirp/stream.video.sfu.signal.SignalServer/SetPublisher", req, Signal.SetPublisherResponse.Parser.ParseFrom);
        }

        // answer is sent by the client to the SFU after receiving a subscriber_offer.
        public static async Task<Signal.SendAnswerResponse> SendAnswer(HttpClient client, Signal.SendAnswerRequest req)
        {
            return await DoRequest<Signal.SendAnswerRequest, Signal.SendAnswerResponse>(client, "/twirp/stream.video.sfu.signal.SignalServer/SendAnswer", req, Signal.SendAnswerResponse.Parser.ParseFrom);
        }

        // SendICECandidate sends an ICE candidate to the client
        public static async Task<Signal.ICETrickleResponse> IceTrickle(HttpClient client, StreamVideo.v1.Sfu.Models.ICETrickle req)
        {
            return await DoRequest<StreamVideo.v1.Sfu.Models.ICETrickle, Signal.ICETrickleResponse>(client, "/twirp/stream.video.sfu.signal.SignalServer/IceTrickle", req, Signal.ICETrickleResponse.Parser.ParseFrom);
        }

        // UpdateSubscribers is used to notify the SFU about the list of video subscriptions
        // TODO: sync subscriptions based on this + update tracks using the dimension info sent by the user
        public static async Task<Signal.UpdateSubscriptionsResponse> UpdateSubscriptions(HttpClient client, Signal.UpdateSubscriptionsRequest req)
        {
            return await DoRequest<Signal.UpdateSubscriptionsRequest, Signal.UpdateSubscriptionsResponse>(client, "/twirp/stream.video.sfu.signal.SignalServer/UpdateSubscriptions", req, Signal.UpdateSubscriptionsResponse.Parser.ParseFrom);
        }

        public static async Task<Signal.UpdateMuteStatesResponse> UpdateMuteStates(HttpClient client, Signal.UpdateMuteStatesRequest req)
        {
            return await DoRequest<Signal.UpdateMuteStatesRequest, Signal.UpdateMuteStatesResponse>(client, "/twirp/stream.video.sfu.signal.SignalServer/UpdateMuteStates", req, Signal.UpdateMuteStatesResponse.Parser.ParseFrom);
        }

        public static async Task<Signal.ICERestartResponse> IceRestart(HttpClient client, Signal.ICERestartRequest req)
        {
            return await DoRequest<Signal.ICERestartRequest, Signal.ICERestartResponse>(client, "/twirp/stream.video.sfu.signal.SignalServer/IceRestart", req, Signal.ICERestartResponse.Parser.ParseFrom);
        }
    }

}