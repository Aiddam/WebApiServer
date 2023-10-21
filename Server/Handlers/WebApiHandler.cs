using Newtonsoft.Json;
using Server.Attributes;
using Server.Extractors;
using Server.Interfaces;
using Server.Interfaces.HandlersAndControllers;
using Server.Route;
using Server.Writers;
using System.Net;
using System.Reflection;

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
        public void Handle(Stream stream, Request request)
        {
            ResponseWriter responseWriter = new();
            if (!_routeRegistry.TryGetRoute(request.Path, out Func<object[], object?>? func))
            {
                responseWriter.Write(HttpStatusCode.NotFound, stream);
            }
            else
            {
                responseWriter.Write(HttpStatusCode.OK, stream);
                WriteControllerResponse(func(new object[] { }), stream);
            }
        }

        public async Task HandleAsync(Stream stream, Request request)
        {
            ResponseWriter responseWriter = new();
            string methodName = request.Path;

            if (_routeDataProcessor.TryGetAttribute(request, out Attribute? attribute))
            {
                HttpGetAttribute? httpGetAttribute = attribute as HttpGetAttribute;
                methodName = "Users/" + httpGetAttribute?.Path;
            }

            if (!_routeRegistry.TryGetRoute(methodName, out Func<object[], object?>? func))
            {
                await responseWriter.WriteAsync(HttpStatusCode.NotFound, stream);
            }
            else
            {
                await responseWriter.WriteAsync(HttpStatusCode.OK, stream);
                object[]? data = _routeDataProcessor.GetData(request, attribute);
                await WriteControllerResponseAsync(func(data!),
                                                   stream);
            }
        }
        #endregion

        #region Write reponse
        private void WriteControllerResponse(object? response, Stream stream)
        {
            if (response is string str)
            {
                using StreamWriter writer = new(stream, leaveOpen: true);
                writer.Write(str);
            }
            else if (response is byte[] buffer)
            {
                stream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                WriteControllerResponse(JsonConvert.SerializeObject(response), stream);
            }
        }

        private async Task WriteControllerResponseAsync(object? response, Stream stream)
        {
            if (response is string str)
            {
                using StreamWriter writer = new(stream, leaveOpen: true);
                await writer.WriteAsync(str);
            }
            else if (response is byte[] buffer)
            {
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            else if (response is Task task)
            {
                TaskExtractor taskExtractor = new();
                await task;
                await WriteControllerResponseAsync(taskExtractor.ExtractValue(task)!, stream);
            }
            else
            {
                await WriteControllerResponseAsync(JsonConvert.SerializeObject(response), stream);
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
        #endregion
    }
}
