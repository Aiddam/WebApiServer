using Server.Interfaces.HandlersAndControllers;
using Server.Models.Enum;
using Server.Parser;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server
{
    record User(int Id, string Name);
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
                _ = ProcessClientAsync(client);
            }
        }

        public async Task StartAsync()
        {
            TcpListener tcpListener = new(IPAddress.Any, port);
            tcpListener.Start();
            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                _ = ProcessClientAsync(client);
            }
        }

        private async ValueTask ProcessClientAsync(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new(stream))
            {
                string? firstLine = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(firstLine))
                {
                    return;
                }
                RequestParser requestParser = new();
                Request request = requestParser.Parse(firstLine!);

                switch (request.HttpMethod)
                {
                    case HttpActionType.Post:
                        string postData = await ReadPostDataAsync(reader);
                        request.DataFromPost = postData.Substring(0);

                        break;  
                    default:
                        for (string? line = null; line != string.Empty; line = await reader.ReadLineAsync()) { };
                        break;
                }

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
        private async Task<string> ReadPostDataAsync(StreamReader reader)
        {
            string contentLengthLine = await ReadHeaderLineAsync(reader, "Content-Length");
            if (string.IsNullOrEmpty(contentLengthLine))
            {
                throw new InvalidOperationException("Content-Length header is missing in the POST request.");
            }

            int.TryParse(contentLengthLine, out int contentLength);

            char[] buffer = new char[contentLength];
            int readLength = await reader.ReadAsync(buffer, 0, contentLength);

            return new string(buffer, 0, readLength) + "}";
        }
        private async Task<string> ReadHeaderLineAsync(StreamReader reader, string headerName)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith(headerName, StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring(headerName.Length + 1).Trim();
                }
                if (line == string.Empty)
                {
                    break;
                }
            }
            return null;
        }
    }

}
