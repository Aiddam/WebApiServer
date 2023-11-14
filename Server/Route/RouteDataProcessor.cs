using Server.Attributes;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

namespace Server.Route
{
    public class RouteDataProcessor
    {
        private readonly RouteRegistry _routeRegistry;
        public RouteDataProcessor(RouteRegistry routeRegistry)
        {
            _routeRegistry = routeRegistry;
        }
        public object[]? GetData(Request request)
        {
            if (!TryGetMethod(request.ControllerName, request.MethodName, out MethodInfo? method))
            {
                return null;
            }

            if (!TryGetHttpAttribute(method!, out Attribute? attribute))
            {
                return null;
            }

            return attribute switch
            {
                HttpGetAttribute _ => ProcessGetRequest(request, method!),
                HttpPostAttribute _ => ProcessPostRequest(request, method!),
                // Add more cases as needed
                _ => null
            };
        }

        public bool TryGetHttpAttribute(Request request, out Attribute? attribute)
        {
            attribute = null;

            if (TryGetMethod(request.ControllerName, request.MethodName, out MethodInfo? method))
            {
                _ = TryGetHttpAttribute(method!, out attribute);
            }
            return false;
        }
        private bool TryGetHttpAttribute(MethodInfo method, out Attribute? attribute)
        {
            attribute = method.GetCustomAttributes()
                              .FirstOrDefault(attr => attr is HttpGetAttribute or HttpPostAttribute);
            return attribute != null;
        }
        public bool TryGetMethod(string controllerName, string methodName, out MethodInfo? method)
        {
            method = null;
            if (controllerName.EndsWith("controller", StringComparison.InvariantCultureIgnoreCase))
            {
                controllerName = controllerName[..^"controller".Length];
            }

            if (_routeRegistry.ControllerMethods.TryGetValue(controllerName, out Dictionary<string, MethodInfo>? controller))
            {
                if (controller.TryGetValue(methodName, out MethodInfo? controllerMethod))
                {
                    method = controllerMethod;
                    return true;
                }
            }
            return false;
        }
        private object[] ProcessGetRequest(Request request, MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] data = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                data[i] = ParseParameter(parameters[i], request?.Data?[i]!);
            }

            return data;
        }
        private object[] ProcessPostRequest(Request request, MethodInfo method)
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
        private object ParseParameter(ParameterInfo parameterInfo, object value)
        {
            return parameterInfo.ParameterType switch
            {
                Type t when t == typeof(int) => int.TryParse(value as string, out int intValue) ? intValue : default,
                Type t when t == typeof(string) => value,
                _ => null!
            };
        }
    }
}
