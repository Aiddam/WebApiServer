using Server.Interfaces.HandlersAndControllers;
using Server.Writers;
using System.Net;

namespace Server.Handlers
{
    public class StaticFileHandler : IHandler
    {
        private readonly string _path;
        public StaticFileHandler(string path)
        {
            _path = path;
        }
        public void Handle(Stream networkStream, Request request)
        {
            using StreamWriter writer = new(networkStream);
            ResponseWriter responseWriter = new();

            string filePath = Path.Combine(_path, request.Path);
            Console.WriteLine($"FilePath: {filePath}");
            Console.WriteLine(File.Exists(filePath));
            if (!File.Exists(filePath))
            {
                responseWriter.Write(HttpStatusCode.NotFound, networkStream);
            }
            else
            {
                responseWriter.Write(HttpStatusCode.OK, networkStream);
                using FileStream fileStream = File.OpenRead(filePath);
                fileStream.CopyTo(networkStream);
            }
        }

        public async Task HandleAsync(Stream networkStream, Request request)
        {

            using StreamReader reader = new(networkStream);
            using StreamWriter writer = new(networkStream);

            ResponseWriter responseWriter = new();

            string filePath = Path.Combine(_path, request.Path);

            if (!File.Exists(filePath))
            {
                await responseWriter.WriteAsync(HttpStatusCode.NotFound, networkStream);
            }
            else
            {
                await responseWriter.WriteAsync(HttpStatusCode.OK, networkStream);
                using FileStream fileStream = File.OpenRead(filePath);
                await fileStream.CopyToAsync(networkStream);
            }
        }
    }
}
