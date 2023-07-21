//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#nullable enable


using StreamVideo.Core.InternalDTO.Requests;
using StreamVideo.Core.InternalDTO.Responses;
using StreamVideo.Core.InternalDTO.Models;

namespace StreamVideo.Core.InternalDTO.Events
{
    using System = global::System;

    /// <summary>
    /// This event is sent to all call members to notify they are getting called
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.19.0.0 (NJsonSchema v10.9.0.0 (Newtonsoft.Json v10.0.0.0))")]
    internal partial class CallRingEvent
    {
        /// <summary>
        /// Call object
        /// </summary>
        [Newtonsoft.Json.JsonProperty("call", Required = Newtonsoft.Json.Required.Always)]
        public CallResponse Call { get; set; } = new CallResponse();

        [Newtonsoft.Json.JsonProperty("call_cid", Required = Newtonsoft.Json.Required.Always)]
        public string CallCid { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("created_at", Required = Newtonsoft.Json.Required.Always)]
        public System.DateTimeOffset CreatedAt { get; set; } = default!;

        /// <summary>
        /// Call members
        /// </summary>
        [Newtonsoft.Json.JsonProperty("members", Required = Newtonsoft.Json.Required.Always)]
        public System.Collections.Generic.List<MemberResponse> Members { get; set; } = new System.Collections.Generic.List<MemberResponse>();

        /// <summary>
        /// Call session ID
        /// </summary>
        [Newtonsoft.Json.JsonProperty("session_id", Required = Newtonsoft.Json.Required.Always)]
        public string SessionId { get; set; } = default!;

        /// <summary>
        /// The type of event: "call.notification" in this case
        /// </summary>
        [Newtonsoft.Json.JsonProperty("type", Required = Newtonsoft.Json.Required.Always)]
        public string Type { get; set; } = "call.ring";

        /// <summary>
        /// The user that sent the call notification
        /// </summary>
        [Newtonsoft.Json.JsonProperty("user", Required = Newtonsoft.Json.Required.Always)]
        public UserResponse User { get; set; } = new UserResponse();

    }

}

