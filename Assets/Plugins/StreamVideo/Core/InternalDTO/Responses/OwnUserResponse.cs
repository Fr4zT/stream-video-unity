//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#nullable enable


using StreamVideo.Core.InternalDTO.Requests;
using StreamVideo.Core.InternalDTO.Events;
using StreamVideo.Core.InternalDTO.Models;

namespace StreamVideo.Core.InternalDTO.Responses
{
    using System = global::System;

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0))")]
    internal partial class OwnUserResponse
    {
        [Newtonsoft.Json.JsonProperty("created_at", Required = Newtonsoft.Json.Required.Always)]
        public System.DateTimeOffset CreatedAt { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("custom", Required = Newtonsoft.Json.Required.Always)]
        public System.Collections.Generic.Dictionary<string, object> Custom { get; set; } = new System.Collections.Generic.Dictionary<string, object>();

        [Newtonsoft.Json.JsonProperty("deleted_at", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset DeletedAt { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("devices", Required = Newtonsoft.Json.Required.Always)]
        public System.Collections.Generic.List<Device> Devices { get; set; } = new System.Collections.Generic.List<Device>();

        [Newtonsoft.Json.JsonProperty("id", Required = Newtonsoft.Json.Required.Always)]
        public string Id { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("image", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Image { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Name { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("role", Required = Newtonsoft.Json.Required.Always)]
        public string Role { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("teams", Required = Newtonsoft.Json.Required.Always)]
        public System.Collections.Generic.List<string> Teams { get; set; } = new System.Collections.Generic.List<string>();

        [Newtonsoft.Json.JsonProperty("updated_at", Required = Newtonsoft.Json.Required.Always)]
        public System.DateTimeOffset UpdatedAt { get; set; } = default!;

    }

}

