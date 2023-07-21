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
    internal partial class CallSessionResponse
    {
        [Newtonsoft.Json.JsonProperty("accepted_by", Required = Newtonsoft.Json.Required.Always)]
        public System.Collections.Generic.Dictionary<string, System.DateTimeOffset> AcceptedBy { get; set; } = new System.Collections.Generic.Dictionary<string, System.DateTimeOffset>();

        [Newtonsoft.Json.JsonProperty("ended_at", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset EndedAt { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("id", Required = Newtonsoft.Json.Required.Always)]
        public string Id { get; set; } = default!;

        [Newtonsoft.Json.JsonProperty("participants", Required = Newtonsoft.Json.Required.Always)]
        public System.Collections.Generic.List<CallParticipantResponse> Participants { get; set; } = new System.Collections.Generic.List<CallParticipantResponse>();

        [Newtonsoft.Json.JsonProperty("participants_count_by_role", Required = Newtonsoft.Json.Required.Always)]
        public System.Collections.Generic.Dictionary<string, int> ParticipantsCountByRole { get; set; } = new System.Collections.Generic.Dictionary<string, int>();

        [Newtonsoft.Json.JsonProperty("rejected_by", Required = Newtonsoft.Json.Required.Always)]
        public System.Collections.Generic.Dictionary<string, System.DateTimeOffset> RejectedBy { get; set; } = new System.Collections.Generic.Dictionary<string, System.DateTimeOffset>();

        [Newtonsoft.Json.JsonProperty("started_at", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset StartedAt { get; set; } = default!;

    }

}

