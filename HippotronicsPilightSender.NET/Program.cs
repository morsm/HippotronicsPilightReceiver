using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Termors.Services.Libraries.PilightSocket;
using Termors.Services.Libraries.Ev1527Lib;


namespace Termors.Services.HippotronicsPilightSender
{
    class Program : PilightDaemon<Configuration>, IDisposable
    {
        public override string Name
        {
            get
            {
                return "HippotronicsPilightSender";
            }
        }

        public override PilightServerconfig Serverconfig
        {
            get
            {
                return new PilightServerconfig { IPAddress = Configuration.Server.IPAddress, Port = Configuration.Server.Port };
            }
        }

        static void Main(string[] args)
        {
            var prog = new Program();

            prog.Init();        // Starts loop

            // After loop
            prog.Dispose();
        }

        public void Dispose()
        {
            LightService.DisposeAll();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();

            // Set up lamp services
            foreach (var lamp in Configuration.Lamps)
            {
                SetupService(lamp);
            }

            foreach (var key in LightService.Registry.Keys)
            {
                Console.WriteLine("Running on port {0}: {1}", key, LightService.Registry[key].Name);
            }
        }

        private void SetupService(LampConfig lamp)
        {
            ushort port = FreeTcpPort();
            var service = new LightService(lamp.Name, port)
            {
                OnCommand = lamp.OnCommand,
                OffCommand = lamp.OffCommand
            };

            service.LampSwitched += Service_LampSwitched;

            service.StartWebserver();
            service.RegisterMDNS();
        }

        private static ushort FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return Convert.ToUInt16(port);
        }

        async Task Service_LampSwitched(LightService service, bool on)
        {
            var cmd = on ? service.OnCommand : service.OffCommand;
            var msg = new PilightRawMessage();

            // Encode command
            int[] pulses = Ev1527Decoder.Encode(cmd.Unitcode, cmd.Operation);
            msg.Code.SetPulsesAsCode(pulses);

            await Socket.SendMesage(msg.ToString());
        }


    }
}
