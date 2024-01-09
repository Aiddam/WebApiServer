using Server.Delegates;
using Server.Interfaces;
using Server.Interfaces.HandlersAndControllers;
using Server.Middlewares;
using Server.Models;
using Server.Models.Enum;
using Server.Parser;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    record User(int Id, string Name);
    public class ServerHost
    {
        private const int port = 80;
        private readonly IHandler _handler;
        public List<MiddlewareDelegate> middlewares = new List<MiddlewareDelegate>();
        public ServerHost(IHandler handler)
        {
            _handler = handler;
            middlewares = new();
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
            UseMiddleware(() => new FinalMiddleware(_handler));
            TcpListener tcpListener = new(IPAddress.Any, port);
            tcpListener.Start();
            await Console.Out.WriteLineAsync("Server started");
            OpenBrowser($"http://localhost:{port}");
            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                _ = ProcessClientAsync(client);
            }
        }

        public void UseMiddleware<T>(Func<T> factory) where T : IMiddleware
        {
            MiddlewareDelegate middlewareDelegate = (context, next) => factory().InvokeAsync(context, next);
            middlewares.Add(middlewareDelegate);
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

                await RunMiddlewares(request, stream);
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
        private void OpenBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to open URL in browser: {ex.Message}");
            }
        }
        private async Task RunMiddlewares(Request request, NetworkStream stream)
        {
            Func<Task> next = () => Task.CompletedTask;
            var context = new ServerContext(request, stream);
            foreach (var middleware in middlewares.AsEnumerable().Reverse())
            {
                var current = next;
                next = () => middleware(context, current);
            }

            await next();
        }

    }

}
