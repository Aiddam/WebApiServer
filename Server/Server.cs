using Server.Interfaces.HandlersAndControllers;
using Server.Parser;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class ServerHost
    {
        private const int port = 80;
        private readonly IHandler _handler;
        public ServerHost(IHandler handler)
        {
            _handler = handler;
        }

        public void Start()
        {
            TcpListener tcpListener = new(IPAddress.Any, port);
            tcpListener.Start();
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                _ = ProcessClient(client);
            }
        }

        public async Task StartAsync()
        {
            TcpListener tcpListener = new(IPAddress.Any, port);
            tcpListener.Start();
            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                _ = ProcessClient(client);
            }
        }

        private async ValueTask ProcessClient(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new(stream))
            {
                string? firstLine = await reader.ReadLineAsync();
                //TODO: Handling other HTTP methods
                //POST: Content-Length: 
                for (string? line = null; line != string.Empty; line = await reader.ReadLineAsync()) { };

                RequestParser requestParser = new();
                Request request = requestParser.Parse(firstLine!);

                try
                {
                    await _handler.HandleAsync(stream, request);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.ToString());
                }
            }
        }
    }

}
