using System;
using System.IO;
using Newtonsoft.Json;


namespace Termors.Services.HippotronicsPilightSender
{
    class Program
    {
        public static Configuration Configuration;

        static void Main(string[] args)
        {
            LoadConfig();
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
