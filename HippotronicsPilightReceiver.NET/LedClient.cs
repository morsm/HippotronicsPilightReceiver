using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Termors.Services.HippotronicsPilightReceiver
{
    internal class LampState
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int NodeType { get; set; }
        public DateTime LastSeen { get; set; }
        public bool Online { get; set; }
        public bool On { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
    }

    public class LedClient
    {
        private readonly string _server;
        private readonly int _port;
        private readonly string _lampname;

        public LedClient(string server, int port, string lampname)
        {
            _server = server;
            _port = port;
            _lampname = lampname;
        }

        protected string Url
        {
            get
            {
                return String.Format("http://{0}:{1}/webapi/lamp/{2}", _server, _port, _lampname);
            }
        }

        public async Task Toggle()
        {
            var state = await GetState();
            state.On = (!state.On);
            await SetState(state);
        }

        private async Task SetState(LampState state)
        {
            var client = new HttpClient();
            var json = JsonConvert.SerializeObject(state);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Url, content);
            response.EnsureSuccessStatusCode();
        }

        private async Task<LampState> GetState()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(Url);
            response.EnsureSuccessStatusCode();     // Throw if HTTP error

            // Read state object
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LampState>(json);

        }
    }
}
