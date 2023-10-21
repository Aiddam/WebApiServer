using Server.Attributes;
using System.Reflection;

namespace Server.Route
{
    public class RouteDataProcessor
    {
        private readonly RouteRegistry _routeRegistry;
        public RouteDataProcessor(RouteRegistry routeRegistry)
        {
            _routeRegistry = routeRegistry;
        }
        public object[]? GetData(Request request, Attribute? attribute)
        {
            _ = TryGetMethod(request.ControllerName, request.MethodName, out MethodInfo? method);
            ParameterInfo[]? parameters = method?.GetParameters();

            if (parameters == null)
            {
                return null;
            }

            object[] data = new object[parameters.Length];

            switch (attribute)
            {
                case HttpGetAttribute:
                    HttpGetAttribute? httpGetAttribute = attribute as HttpGetAttribute;
                    _ = httpGetAttribute?.MethodParameters;

                    for (int parameter = 0; parameter < request?.Data?.Length; parameter++)
                    {
                        if (parameters[parameter].ParameterType == typeof(int))
                        {
                            _ = int.TryParse((string)request.Data[parameter], out int value);
                            data[parameter] = value;
                        }
                        else if (parameters[parameter].ParameterType == typeof(string))
                        {
                            data[parameter] = request.Data[parameter];
                        }
                    }
                    break;
            }

            return data;
        }

        public bool TryGetAttribute(Request request, out Attribute? attribute)
        {
            attribute = null;

            if (TryGetMethod(request.ControllerName, request.MethodName, out MethodInfo? method))
            {
                IEnumerable<Attribute>? MethodAtributtes = method?.GetCustomAttributes();
                if (MethodAtributtes!.Any(m => m.GetType() == typeof(HttpGetAttribute)))
                {
                    attribute = method?.GetCustomAttribute<HttpGetAttribute>();
                    return true;
                }
                //TODO other methods
            }
            return false;
        }
        public bool TryGetMethod(string controllerName, string methodName, out MethodInfo? method)
        {
            method = null;

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
    }
}
