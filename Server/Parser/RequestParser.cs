using Server.Interfaces.Parsers;
using Server.Models.Enum;
using System.Net;

namespace Server.Parser
{
    public class RequestParser : IParser
    {
        public Request Parse(string header)
        {
            if (string.IsNullOrEmpty(header))
            {
                throw new ArgumentNullException(nameof(header), "Header cannot be null or empty.");
            }
            string[] split = header.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 2)
            {
                throw new FormatException("Header format is invalid. Expected at least two parts.");
            }

            string method = split[0];
            string url = WebUtility.UrlDecode(split[1]);
            string[] pathSegments = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return new Request(
                url,
                GetControllerName(pathSegments),
                GetMethod(method),
                GetMethodName(pathSegments),
                GetDataFromHeader(pathSegments));
        }

        private static HttpActionType GetMethod(string method)
        {
            return method.Trim() switch
            {
                "GET" => HttpActionType.Get,
                "POST" => HttpActionType.Post,
                "PUT" => HttpActionType.Put,
                "DELETE" => HttpActionType.Delete,
                "HEAD" => HttpActionType.Get,
                _ => throw new NotSupportedException($"Unsupported HTTP method: {method}"),
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
