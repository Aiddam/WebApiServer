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

            Type parameterType = parameters[0].ParameterType;
            object? deserializedData = JsonSerializer.Deserialize(request?.DataFromPost!, parameterType);

            return new object[] { deserializedData! };
        }
    }
}
