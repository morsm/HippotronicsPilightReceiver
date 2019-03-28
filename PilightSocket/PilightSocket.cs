using System;
using System.Threading;
using System.Threading.Tasks;


namespace Termors.Services.Libraries.PilightSocket
{
    public delegate Task MessageListener(PilightMessage msg);

    public class PilightSocket : IDisposable
    {
        public TcpTextClient Client { get; private set; }
        protected ManualResetEvent QuitEvent = new ManualResetEvent(false);

        public event MessageListener MessageReceived;

        public PilightSocket(string ip, int port)
        {
            Client = new TcpTextClient(ip, port);
        }

        public void SocketLoop()
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

        public void Dispose()
        {
            QuitEvent.Set();
            Client.Dispose();
        }
    }
}
