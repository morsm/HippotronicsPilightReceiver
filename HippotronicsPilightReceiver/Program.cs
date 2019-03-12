using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Termors.Services.HippotronicsPilightReceiver
{
    class Program
    {
        public static Configuration Configuration;
        public static TcpTextClient Client;

        public static readonly string PILIGHT_ID_MESSAGE = "{ \"action\": \"identify\", \"options\": { \"receiver\": 1 } }";


        static void Main(string[] args)
        {
            var quit = new System.Threading.ManualResetEvent(false);

            Console.WriteLine("HippotronicsPilightReceiver started");

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("SIGINT received");
                quit.Set();
            };

            LoadConfig();

            SetupSocket();

            // Socket loop
            var quitTask = Task.Run(() => { quit.WaitOne(); });
            var running = true;
            while (running)
            {
                var lineTask = Client.ReadLine();
                if (0 == Task.WaitAny(quitTask, lineTask)) running = false;
                else
                {
                    // line from socket
                    var line = lineTask.Result;

                    // TODO: implement properly
                    Console.WriteLine(line);
                }
            } 

            Console.WriteLine("HippotronicsPilightReceiver stopped");
            Client.Dispose();
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
