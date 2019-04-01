using System;
using Newtonsoft.Json;

namespace Termors.Services.HippotronicsPilightSender
{
    public class LampCommand
    {
        [JsonProperty("unitcode")]
        public uint Unitcode { get; set; }

        [JsonProperty("command")]
        public uint Operation { get; set; }
    }

}
