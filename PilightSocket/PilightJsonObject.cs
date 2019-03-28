using System;
using Newtonsoft.Json;

namespace Termors.Services.Libraries.PilightSocket
{
    public class PilightJsonObject
    {
        [JsonProperty("message")]
        public PJOMessageHeader Message { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }
    }

    public class PJOMessageHeader
    {
        [JsonProperty("systemcode")]
        public int SystemCode { get; set; }

        [JsonProperty("unitcode")]
        public int UnitCode { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}
