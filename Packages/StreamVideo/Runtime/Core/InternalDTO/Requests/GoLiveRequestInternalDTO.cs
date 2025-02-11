//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#nullable enable


using StreamVideo.Core.InternalDTO.Responses;
using StreamVideo.Core.InternalDTO.Events;
using StreamVideo.Core.InternalDTO.Models;

namespace StreamVideo.Core.InternalDTO.Requests
{
    using System = global::System;

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0))")]
    internal partial class GoLiveRequestInternalDTO
    {
        [Newtonsoft.Json.JsonProperty("recording_storage_name", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string RecordingStorageName { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("start_closed_caption", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool StartClosedCaption { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("start_hls", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool StartHls { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("start_recording", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool StartRecording { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("start_rtmp_broadcasts", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool StartRtmpBroadcasts { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("start_transcription", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool StartTranscription { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("transcription_storage_name", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string TranscriptionStorageName { get; set; } = default!;

    }

}

