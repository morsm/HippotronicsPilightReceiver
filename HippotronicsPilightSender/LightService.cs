using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

using Makaretu.Dns;
using Microsoft.Owin.Hosting;
using Owin;

namespace Termors.Services.HippotronicsPilightSender
{
    public delegate void LampSwitchedDelegate(LightService service, bool on);

    public class LightService : IDisposable
    {
        private readonly ushort _port;
        private IDisposable _webapp = null;
        private bool _on;

        public LightService(string name, ushort port)
        {
            Name = name;
            _port = port;
        }

        public event LampSwitchedDelegate LampSwitched;
        public static readonly IDictionary<ushort, LightService> Registry = new Dictionary<ushort, LightService>();

        public void RegisterMDNS()
        {
            var service = new ServiceProfile("HippoLed-" + Name, "_hippohttp._tcp", _port);
            var sd = new ServiceDiscovery();
            sd.Advertise(service);
        }

        public void StartWebserver()
        {
            string url = "http://*:" + _port;
            _webapp = WebApp.Start(url, new Action<IAppBuilder>(WebConfiguration));

            Registry[_port] = this;
        }

        public async Task SetRGB(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;

            // Do nothing. This service only supports on/off commands
        }

        public bool On
        {
            get
            {
                return _on;
            }
            set
            {
                bool oldStatus = _on;
                if (oldStatus != value)
                {
                    _on = value;

                    LampSwitched?.Invoke(this, _on);
                }
            }
        }

        public byte Red { get; private set; } = 127;
        public byte Green { get; private set; } = 127;
        public byte Blue { get; private set; } = 127;
        public string Name { get; private set; }

        public LampCommand OnCommand { get; set; }
        public LampCommand OffCommand { get; set; }


        public void Dispose()
        {
            if (_webapp != null) _webapp.Dispose();
        }

        public static void DisposeAll()
        {
            foreach (var svc in Registry.Values) svc.Dispose();
            Registry.Clear();
        }


        // This code configures Web API using Owin
        private void WebConfiguration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            // Format to JSON by default
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.EnsureInitialized();

            appBuilder.UseWebApi(config);
        }

    }
}
