//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#nullable enable


using StreamVideo.Core.InternalDTO.Requests;
using StreamVideo.Core.InternalDTO.Responses;
using StreamVideo.Core.InternalDTO.Models;

namespace StreamVideo.Core.InternalDTO.Events
{
    using System = global::System;

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.20.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0))")]
    internal partial class CallEventInternalDTO
    {
        [Newtonsoft.Json.JsonProperty("category", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Category { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("component", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Component { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("description", Required = Newtonsoft.Json.Required.Default)]
        public string Description { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("end_timestamp", Required = Newtonsoft.Json.Required.Default)]
        public int EndTimestamp { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("internal", Required = Newtonsoft.Json.Required.Default)]
        public bool Internal { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("issue_tags", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.List<string> IssueTags { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("kind", Required = Newtonsoft.Json.Required.Default)]
        public string Kind { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("severity", Required = Newtonsoft.Json.Required.Default)]
        public int Severity { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("timestamp", Required = Newtonsoft.Json.Required.Default)]
        public int Timestamp { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("type", Required = Newtonsoft.Json.Required.Default)]
        public string Type { get; set; } = default!;

    }

}

