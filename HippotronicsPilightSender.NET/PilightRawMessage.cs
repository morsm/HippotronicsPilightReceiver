using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Termors.Services.HippotronicsPilightSender
{
    public class PilightCode
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("protocol")]
        public string[] Protocol { get; set; } = new string[] { "raw" };

        public void SetPulsesAsCode(int[] pulses)
        {
            StringBuilder sb = new StringBuilder();

            for (int i=0; i<pulses.Length; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append(pulses[i].ToString());
            }

            Code = sb.ToString();
        }
    }

    public class PilightRawMessage
    {
        [JsonProperty("action")]
        public string Message { get; set; } = "send";

        [JsonProperty("code")]
        public PilightCode Code { get; set; } = new PilightCode();

        public override string ToString()
        {
            var ser = JsonSerializer.Create();
            var sw = new StringWriter();
            ser.Serialize(sw, this);

            return sw.ToString();
        }
    }
}
