//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#nullable enable


using StreamVideo.Core.InternalDTO.Requests;
using StreamVideo.Core.InternalDTO.Events;
using StreamVideo.Core.InternalDTO.Models;

namespace StreamVideo.Core.InternalDTO.Responses
{
    using System = global::System;

    /// <summary>
    /// BroadcastSettingsResponse is the payload for broadcasting settings
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0))")]
    internal partial class BroadcastSettingsResponseInternalDTO
    {
        [Newtonsoft.Json.JsonProperty("enabled", Required = Newtonsoft.Json.Required.Default)]
        public bool Enabled { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("hls", Required = Newtonsoft.Json.Required.Default)]
        public HLSSettingsResponseInternalDTO Hls { get; set; } = new HLSSettingsResponseInternalDTO();

        [Newtonsoft.Json.JsonProperty("rtmp", Required = Newtonsoft.Json.Required.Default)]
        public RTMPSettingsResponseInternalDTO Rtmp { get; set; } = new RTMPSettingsResponseInternalDTO();

    }

}

