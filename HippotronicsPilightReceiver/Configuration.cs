using System;
using Newtonsoft.Json;

namespace Termors.Services.HippotronicsPilightReceiver
{
    public enum SwitchType
    {
        ev1527
    }

    public class ServerConfig
    {
        [JsonProperty("ip")]
        public string IPAddress { get; set; }

        [JsonProperty("port")]
        public UInt16 Port { get; set; }
    }

    public class SwitchConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public SwitchType Type { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("lamps")]
        public string[] Lamps { get; set; }
    }

    public class Configuration
    {
        [JsonProperty("server")]
        public ServerConfig Server { get; set; }

        [JsonProperty("switches")]
        public SwitchConfig[] Switches { get; set; }

    }
}
