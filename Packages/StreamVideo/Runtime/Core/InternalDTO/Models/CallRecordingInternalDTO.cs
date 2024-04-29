//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#nullable enable


using StreamVideo.Core.InternalDTO.Requests;
using StreamVideo.Core.InternalDTO.Responses;
using StreamVideo.Core.InternalDTO.Events;

namespace StreamVideo.Core.InternalDTO.Models
{
    using System = global::System;

    /// <summary>
    /// CallRecording represents a recording of a call.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0))")]
    internal partial class CallRecordingInternalDTO
    {
        [Newtonsoft.Json.JsonProperty("end_time", Required = Newtonsoft.Json.Required.Default)]
        public System.DateTimeOffset EndTime { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("filename", Required = Newtonsoft.Json.Required.Default)]
        public string Filename { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("start_time", Required = Newtonsoft.Json.Required.Default)]
        public System.DateTimeOffset StartTime { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("url", Required = Newtonsoft.Json.Required.Default)]
        public string Url { get; set; } = default!;

    }

}

