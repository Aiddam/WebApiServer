using System.Reflection;
using System.Text.Json;

namespace Server.Route.RequestProccessors
{
    public class HttpPostRequestProcessor : BaseRequestProcessor
    {
        public override object[] ProcessRequest(Request request, MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length == 0)
            {
                return Array.Empty<object>();
            }

            object[] data = new object[parameters.Length];
            if (request?.DataFromPost == null)
            {
                throw new ArgumentNullException(nameof(request.DataFromPost), "Data from POST request is null.");
            }

            Type parameterType = parameters[0].ParameterType;
            try
            {
                object deserializedData = JsonSerializer.Deserialize(request.DataFromPost, parameterType)
                                     ?? throw new InvalidOperationException("Deserialized data is null.");
                data[0] = deserializedData;
            }
            catch (JsonException ex)
            {
                throw new JsonException("Error deserializing data.", ex);
            }

            for (int i = 1; i < parameters.Length; i++)
            {
                if (request?.Data?.Length > i - 1)
                {
                    data[i] = ParseParameter(parameters[i], request.Data[i - 1]);
                }
                else
                {
                    throw new ArgumentException($"Missing data for parameter {parameters[i].Name}");
                }
            }

            return data;
        }
    }
}
