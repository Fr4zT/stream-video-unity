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
    internal partial class SDKUsageReportInternalDTO
    {
        [Newtonsoft.Json.JsonProperty("per_sdk_usage", Required = Newtonsoft.Json.Required.Default)]
        public System.Collections.Generic.Dictionary<string, PerSDKUsageReportInternalDTO?> PerSdkUsage { get; set; } = new System.Collections.Generic.Dictionary<string, PerSDKUsageReportInternalDTO?>();

    }

}

