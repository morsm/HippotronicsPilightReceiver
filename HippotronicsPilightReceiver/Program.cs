using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Termors.Services.HippotronicsPilightReceiver
{
    public delegate Task MessageListener(PilightMessage msg);

    class Program
    {
        public static Configuration Configuration;
        public static TcpTextClient Client;
        public static ManualResetEvent QuitEvent = new ManualResetEvent(false);
        public static Dictionary<string, DateTime> CoolOff = new Dictionary<string, DateTime>();

        public static event MessageListener MessageReceived;

        public static readonly string PILIGHT_ID_MESSAGE = "{ \"action\": \"identify\", \"options\": { \"receiver\": 1 } }";


        static void Main(string[] args)
        {
            Console.WriteLine("HippotronicsPilightReceiver started");

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("SIGINT received");
                QuitEvent.Set();
            };

            MessageReceived += OnMessage;

            LoadConfig();

            SetupSocket();

            SocketLoop();

            Console.WriteLine("HippotronicsPilightReceiver stopped");
            Client.Dispose();
        }

        private static async Task OnMessage(PilightMessage msg)
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


        private static void SocketLoop()
        {
            // Socket loop
            var quitTask = Task.Run(() => { QuitEvent.WaitOne(); });
            var running = true;
            var currentMessage = new PilightMessage();

            while (running)
            {
                var lineTask = Client.ReadLine();

                if (0 == Task.WaitAny(quitTask, lineTask))
                {
                    running = false;
                }
                else
                {
                    // line from socket
                    var line = lineTask.Result;

                    currentMessage.AddMessageLine(line);
                    if (currentMessage.IsComplete)
                    {
                        MessageReceived?.Invoke(currentMessage);
                        currentMessage = new PilightMessage();

                    }
                }
            }
        }

        private static void SetupSocket()
        {
            Client = new TcpTextClient(
                Configuration.Server.IPAddress, 
                Configuration.Server.Port
            );

            Client.Send(PILIGHT_ID_MESSAGE).Wait();
        }



        private static void LoadConfig()
        {
            using (var reader = File.OpenText("config.json"))
            {
                var ser = JsonSerializer.Create();
                var jreader = new JsonTextReader(reader);

                Configuration = ser.Deserialize<Configuration>(jreader);
            }
        }
    }
}
