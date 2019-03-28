
using Termors.Services.Libraries.PilightSocket;


namespace Termors.Services.HippotronicsPilightSender
{
    class Program :PilightDaemon<Configuration>
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
            new Program().Init();
        }

    }
}
