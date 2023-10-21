using Server.Interfaces.Parsers;

namespace Server.Parser
{
    public class RequestParser : IParser
    {
        public Request Parse(string header)
        {
            string[] split = header.Trim().Split(" ");
            string method = split[0];
            string[] partUrl = split[1].Split("/").Skip(1).ToArray();

            return new Request(split[1], GetControllerName(partUrl), GetMethod(method), GetMethodName(partUrl), GetDataFromHeader(partUrl));
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

        private string GetControllerName(string[] pathSegments)
        {
            return pathSegments.Length > 0 ? pathSegments[0] + "Controller" : string.Empty;
        }
        private string GetMethodName(string[] pathSegments)
        {
            return pathSegments.Length > 1 ? pathSegments[1] : string.Empty;
        }

        private object[] GetDataFromHeader(string[] pathSegments)
        {
            return pathSegments.Length > 2 ? pathSegments.Skip(2).ToArray() : Array.Empty<object>();
        }
    }
}
