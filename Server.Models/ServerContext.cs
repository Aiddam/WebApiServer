using System.Net.Sockets;

namespace Server.Models
{
    public class ServerContext
    {
        public Request Request { get; set; }
        public NetworkStream Stream { get; set; }
        public ServerContext(Request request, NetworkStream stream)
        {
            Request = request;
            Stream = stream;
        }
    }
}
