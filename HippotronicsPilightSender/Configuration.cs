using System;
using Newtonsoft.Json;

namespace Termors.Services.HippotronicsPilightSender
{

    public class ServerConfig
    {
        [JsonProperty("ip")]
        public string IPAddress { get; set; }

        [JsonProperty("port")]
        public UInt16 Port { get; set; }
    }

    public class LampConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("on")]
        public LampCommand OnCommand { get; set; }

        [JsonProperty("off")]
        public LampCommand OffCommand { get; set; }

    }

    public class Configuration
    {
        [JsonProperty("server")]
        public ServerConfig Server { get; set; }

        [JsonProperty("lamps")]
        public LampConfig[] Lamps { get; set; }

    }
}
