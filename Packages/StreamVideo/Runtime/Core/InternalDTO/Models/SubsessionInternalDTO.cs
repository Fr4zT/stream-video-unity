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

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0))")]
    internal partial class SubsessionInternalDTO
    {
        [Newtonsoft.Json.JsonProperty("sfu_id", Required = Newtonsoft.Json.Required.Default)]
        public string SfuId { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("ts_ended_at", Required = Newtonsoft.Json.Required.Default)]
        public long TsEndedAt { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("ts_joined_at", Required = Newtonsoft.Json.Required.Default)]
        public long TsJoinedAt { get; set; } = default!;

    }

}

