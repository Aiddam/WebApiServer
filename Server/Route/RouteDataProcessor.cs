using Server.Attributes;
using Server.Interfaces;
using Server.Route.RequestProccessors;
using System.Reflection;

namespace Server.Route
{
    public class RouteDataProcessor
    {
        private readonly RouteRegistry _routeRegistry;
        private readonly Dictionary<Type, IRequestProcessor> _requestProcessors;
        private const string ControllerSuffix = "controller";
        public RouteDataProcessor(RouteRegistry routeRegistry)
        {
            _routeRegistry = routeRegistry;
            _requestProcessors = new Dictionary<Type, IRequestProcessor>
        {
            { typeof(HttpGetAttribute), new HttpGetRequestProcessor() },
            { typeof(HttpPostAttribute), new HttpPostRequestProcessor() }
        };
        }
        public object[]? GetData(Request request)
        {
            if (!TryGetMethod(request.ControllerName, request.MethodName, out MethodInfo? method))
            {
                return null;
            }

            var attributeType = method!.GetCustomAttributes()
                                      .FirstOrDefault(attr => attr is BaseHttpMethodAttribute)?.GetType();

            if (attributeType != null && _requestProcessors.TryGetValue(attributeType, out IRequestProcessor? processor))
            {
                return processor.ProcessRequest(request, method!);
            }
            return null;
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
            if (controllerName.EndsWith(ControllerSuffix, StringComparison.InvariantCultureIgnoreCase))
            {
                controllerName = controllerName[..^ControllerSuffix.Length];
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
    }
}
