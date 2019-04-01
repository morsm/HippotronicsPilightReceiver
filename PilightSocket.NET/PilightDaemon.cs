using System;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Termors.Services.Libraries.PilightSocket
{
    public sealed class PilightServerconfig
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
    }

    public abstract class PilightDaemon<T>
    {
        public T Configuration;
        public PilightSocket Socket;

        public abstract string Name { get; }
        public abstract PilightServerconfig Serverconfig { get; }

        public virtual string PILIGHT_ID_MESSAGE
        {
            get {
                return "{ \"action\": \"identify\", \"options\": { \"receiver\": 1 } }";
            }
        }


        public virtual void Init()
        {
            Console.WriteLine("{0} started", Name);

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("SIGINT received");
                Socket.Dispose();
            };

            LoadConfig();

            SetupSocket();

            Socket.SocketLoop();        // Also handles quit event

            Console.WriteLine("{0} stopped", Name);

        }

        protected virtual void LoadConfig()
        {
            using (var reader = File.OpenText("config.json"))
            {
                var ser = JsonSerializer.Create();
                var jreader = new JsonTextReader(reader);

                Configuration = ser.Deserialize<T>(jreader);
            }
        }

        protected virtual void SetupSocket()
        {
            Socket = new PilightSocket(
                Serverconfig.IPAddress,
                Serverconfig.Port
            );

            Socket.SendMesage(PILIGHT_ID_MESSAGE).Wait();

            Socket.MessageReceived += OnMessage;

        }

        public virtual async Task OnMessage(PilightMessage msg)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<PilightJsonObject>(msg.Message);

                // Default implementation does nothing except log
                Console.WriteLine("Message from Pilight: {0}", msg.Message);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception parsing pilight message: {0} {1}", ex.GetType().Name, ex.Message);
            }
        }



    }
}
