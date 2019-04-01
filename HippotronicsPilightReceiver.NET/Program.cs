using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Termors.Services.Libraries.PilightSocket;


namespace Termors.Services.HippotronicsPilightReceiver
{

    class Program : PilightDaemon<Configuration>
    {
        public static Dictionary<string, DateTime> CoolOff = new Dictionary<string, DateTime>();

        static void Main(string[] args)
        {
            new Program().Init();
        }

        public override async Task OnMessage(PilightMessage msg)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<PilightJsonObject>(msg.Message);

                // Only handle messages received from switches
                if (json.Origin != "receiver") return;

                // Find this object in the config
                var switches = from sw in Configuration.Switches
                               where sw.Type.ToString() == json.Protocol &&
                                     sw.Address == json.Message.UnitCode.ToString() &&      // TODO: other identifiers for other protocols
                                     sw.State == json.Message.State
                               select sw;

                foreach (var sw in switches)
                {
                    Console.WriteLine("Message received from switch {0}", sw.Name);
                    List<Task> tasks = new List<Task>();

                    foreach (var lamp in sw.Lamps)
                    {
                        // Was this lamp recently switched?
                        var now = DateTime.Now;
                        if (CoolOff.ContainsKey(lamp) && (now - CoolOff[lamp]).TotalMilliseconds < 1000)
                        {
                            // Update cooloff, to prevent issues with long presses
                            // TODO: dim?
                            CoolOff[lamp] = now;
                        }
                        else
                        {
                            Console.WriteLine("Switching " + lamp);

                            var client = new LedClient(Configuration.HippoLed.IPAddress, Configuration.HippoLed.Port, lamp);
                            tasks.Add(client.Toggle());

                            CoolOff[lamp] = now;
                        }
                    }

                    await Task.WhenAll(tasks.ToArray());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception parsing pilight message: {0} {1}", ex.GetType().Name, ex.Message);
            }
        }


        public override string Name
        {
            get
            {
                return "HippotronicsPilightReceiver";
            }
        }

        public override PilightServerconfig Serverconfig
        {
            get
            {
                return new PilightServerconfig { IPAddress = Configuration.Server.IPAddress, Port = Configuration.Server.Port };
            }
        }


    }
}
