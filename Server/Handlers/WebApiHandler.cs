using Newtonsoft.Json;
using Server.Attributes;
using Server.Extractors;
using Server.Interfaces;
using Server.Interfaces.HandlersAndControllers;
using Server.Route;
using Server.Writers;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Server.Handlers
{
    public class WebApiHandler : IHandler, IDependencyService
    {
        #region fields and ctor
        private readonly RouteRegistry _routeRegistry;
        private readonly RouteDataProcessor _routeDataProcessor;
        public WebApiHandler(Assembly controllersAssembly)
        {
            _routeRegistry = new RouteRegistry(controllersAssembly);
            _routeDataProcessor = new RouteDataProcessor(_routeRegistry);
        }
        #endregion

        #region Handle

        public async Task HandleAsync(Stream stream, Request request)
        {
            ResponseWriter responseWriter = new();

            string pattern = "Controller";
            string controllerName = Regex.Replace(request.ControllerName, pattern, "", RegexOptions.IgnoreCase);

            var methodName = ResolveMethodName(request);

            if (_routeDataProcessor.TryGetHttpAttribute(request, out Attribute? attribute))
            {
                HttpGetAttribute? httpGetAttribute = attribute as HttpGetAttribute;
                if (!string.IsNullOrEmpty(httpGetAttribute?.Path))
                {
                    methodName = controllerName + "/" + httpGetAttribute?.Path;
                }
            }

            if (!_routeRegistry.TryGetRoute(methodName, out Func<object[], object?>? func))
            {
                await responseWriter.WriteAsync(HttpStatusCode.NotFound, stream);
                return;
            }

            await responseWriter.WriteAsync(HttpStatusCode.OK, stream);

            object[]? data = _routeDataProcessor.GetData(request);

            await WriteControllerResponseAsync(func(data!),
                                                   stream);
        }
        #endregion

        #region Write reponse

        private async Task WriteControllerResponseAsync(object? response, Stream stream)
        {
            if (response == null) return;

            switch (response)
            {
                case string str:
                    {
                        using StreamWriter writer = new(stream, leaveOpen: true);
                        await writer.WriteAsync(str);
                        break;
                    }

                case byte[] buffer:
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    break;

                case Task task:
                    {
                        TaskExtractor taskExtractor = new();
                        await task;
                        await WriteControllerResponseAsync(taskExtractor.ExtractValue(task)!, stream);
                        break;
                    }

                default:
                    await WriteControllerResponseAsync(JsonConvert.SerializeObject(response), stream);
                    break;
            }
        }
        #endregion
        #region TODO container 

        public void AddTransient<TInterface, TType>() where TInterface : class where TType : class
        {
            AddTransient(typeof(TInterface), typeof(TType));
        }

        public void AddTransient(Type typeInterface, Type type)
        {
            _routeRegistry.AddTransient(typeInterface, type);
        }
        public void AddSingleton<TInterface, TType>() where TInterface : class where TType : class
        {
            AddSingleton(typeof(TInterface), typeof(TType));
        }
        public void AddSingleton(Type typeInterface, Type type)
        {
            _routeRegistry.AddSingleton(typeInterface, type);
        }
        #endregion
        private string ResolveMethodName(Request request)
        {
            var controllerName = request.ControllerName.Replace("Controller", "", StringComparison.OrdinalIgnoreCase);
            return string.IsNullOrEmpty(request.MethodName)
                ? $"/{controllerName}"
                : $"/{controllerName}/{request.MethodName}";
        }
    }
}
