using System;
using System.Net;
using System.Net.Sockets;

using Termors.Services.Libraries.PilightSocket;


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

        void Service_LampSwitched(LightService service, bool on)
        {
            // TODO
        }

    }
}
