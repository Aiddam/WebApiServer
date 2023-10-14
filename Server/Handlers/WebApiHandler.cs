using Newtonsoft.Json;
using Server.Extractors;
using Server.Interfaces;
using Server.Interfaces.HandlersAndControllers;
using Server.Writers;
using System.Net;
using System.Reflection;

namespace Server.Handlers
{
    public class WebApiHandler : IHandler, IDependencyService
    {
        private readonly Dictionary<string, Func<object[], object?>> _routes;
        private readonly Dictionary<Type, Type> _types;
        private readonly InstanceCreator _instanceCreator;
        public WebApiHandler(Assembly controllersAssembly)
        {
            _routes = controllersAssembly.GetTypes()
                .Where(type => typeof(IController).IsAssignableFrom(type))
                .SelectMany(Controller => Controller.GetMethods().Where(method => method.DeclaringType != typeof(object))
                .Select(Method => new
                {
                    Controller,
                    Method
                }))
                .ToDictionary(
                key => GetPath(key.Controller, key.Method),
                value => GetEndpoint(value.Controller, value.Method)
                );

            _types = new Dictionary<Type, Type>();
            _instanceCreator = new InstanceCreator();
        }

        public void Handle(Stream stream, Request request)
        {
            ResponseWriter responseWriter = new();
            if (!_routes.TryGetValue(request.Path, out Func<object[], object?>? func))
            {
                responseWriter.Write(HttpStatusCode.NotFound, stream);
            }
            else
            {
                responseWriter.Write(HttpStatusCode.OK, stream);
                WriteControllerResponse(func(new object[] { })!, stream);
            }
        }

        public async Task HandleAsync(Stream stream, Request request)
        {
            ResponseWriter responseWriter = new();
            if (!_routes.TryGetValue(request.Path, out Func<object[], object?>? func))
            {
                await responseWriter.WriteAsync(HttpStatusCode.NotFound, stream);
            }
            else
            {
                await responseWriter.WriteAsync(HttpStatusCode.OK, stream);
                await WriteControllerResponseAsync(func(new object[] { })!, stream);
            }
        }

        public void AddTransient<TInterface, TType>() where TInterface : class where TType : class
        {
            AddTransient(typeof(TInterface), typeof(TType));
        }

        public void AddTransient(Type typeInterface, Type type)
        {
            _types.Add(typeInterface, type);
        }

        private Func<object[], object?> GetEndpoint(Type controller, MethodInfo method)
        {
            ParameterInfo[]? parameters = controller.GetConstructors()?.FirstOrDefault()?.GetParameters();
            Type[] types = new Type[parameters!.Length];
            for (int parametr = 0; parametr < parameters.Length; parametr++)
            {
                types[parametr] = parameters[parametr].ParameterType;
            }
            return (data) => method.Invoke(_instanceCreator.CreateInstance(controller, types, _types), data);
        }

        private string GetPath(Type controller, MethodInfo method)
        {
            string name = controller.Name;
            if (name.EndsWith("controller", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name[..^"controller".Length];
            }
            return method.Name.Equals("Index", StringComparison.InvariantCultureIgnoreCase) ? "/" + name : "/" + name + "/" + method.Name;
        }
        private void WriteControllerResponse(object response, Stream stream)
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

        private async Task WriteControllerResponseAsync(object response, Stream stream)
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
    }
}
