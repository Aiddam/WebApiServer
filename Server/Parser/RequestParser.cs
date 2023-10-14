using Server.Interfaces.Parsers;

namespace Server.Parser
{
    public class RequestParser : IParser
    {
        public Request Parse(string header)
        {
            string[] split = header.Trim().Split(" ");

            return new Request(split[1], GetMethod(split[0]));
        }

        private static HttpMethod GetMethod(string method)
        {
            return method.Trim() switch
            {
                "GET" => HttpMethod.Get,
                "POST" => HttpMethod.Post,
                "PUT" => HttpMethod.Put,
                "DELETE" => HttpMethod.Delete,
                "HEAD" => HttpMethod.Get,
                _ => HttpMethod.Post,
            };
        }
    }
}
