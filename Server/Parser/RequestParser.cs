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

        private static string GetControllerName(string[] pathArray)
        {
            return pathArray[0] + "Controller";
        }
        private static string GetMethodName(string[] pathArray)
        {
            return pathArray.Length < 2 ? pathArray[0] : pathArray[1];
        }

        private static object[] GetDataFromHeader(string[] header)
        {
            IEnumerable<string> data = header.Skip(2);

            return data.ToArray();
        }
    }
}
