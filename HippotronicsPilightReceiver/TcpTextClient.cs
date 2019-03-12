using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Termors.Services.HippotronicsPilightReceiver
{
    public class TcpTextClient : TcpClient
    {
        protected readonly StreamWriter _writer;
        protected readonly StreamReader _reader;

        public TcpTextClient(string ip, int port)
            : base(ip, port)
        {
            _reader = new StreamReader(GetStream());
            _writer = new StreamWriter(GetStream());
        }

        public async Task Send(string str)
        {
            await _writer.WriteLineAsync(str);
            _writer.Flush();
        }

        public async Task<string> ReadLine()
        {
            return await _reader.ReadLineAsync();
        }
    }
}
