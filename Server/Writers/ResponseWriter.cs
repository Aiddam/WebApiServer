using Server.Interfaces.Writers;
using System.Net;

namespace Server.Writers
{
    public class ResponseWriter : IWriterStream<HttpStatusCode>
    {
        public void Write(HttpStatusCode value, Stream stream)
        {
            using StreamWriter writer = new(stream, leaveOpen: true);
            writer.WriteLine($"HTTP/1.1 {(int)value} {value}");
            writer.WriteLine();
        }

        public async Task WriteAsync(HttpStatusCode value, Stream stream)
        {
            using StreamWriter writer = new(stream, leaveOpen: true);
            await writer.WriteLineAsync($"HTTP/1.1 {(int)value} {value}");
            await writer.WriteLineAsync();
        }
    }
}
