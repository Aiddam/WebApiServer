using Server.Attributes;
using Server.Extractors;
using Server.Interfaces.HandlersAndControllers;
using System.Reflection;
using System.Xml.Linq;

public class RouteRegistry
{
    private readonly Dictionary<string, Func<object[], object?>> _routes;

    private readonly Dictionary<string, Dictionary<string, MethodInfo>> _controllerMethods;
    public IReadOnlyDictionary<string, Dictionary<string, MethodInfo>> ControllerMethods => _controllerMethods;

    private readonly InstanceCreator _instanceCreator;
    public Dictionary<Type, Type> Types;

    public RouteRegistry(Assembly controllersAssembly)
    {
        _instanceCreator = new InstanceCreator();
        _controllerMethods = new Dictionary<string, Dictionary<string, MethodInfo>>();
        _routes = CreateRouteDictionary(controllersAssembly);

        Types = new Dictionary<Type, Type>();
    }

    public bool TryGetRoute(string path, out Func<object[], object?>? route)
    {
        return _routes.TryGetValue(path, out route);
    }

    #region Get information about the route
    private Func<object[], object?> GetEndpoint(Type controller, MethodInfo method)
    {
        ParameterInfo[]? parameters = controller.GetConstructors()?.FirstOrDefault()?.GetParameters();
        Type[] types = new Type[parameters!.Length];
        for (int parametr = 0; parametr < parameters.Length; parametr++)
        {
            types[parametr] = parameters[parametr].ParameterType;
        }
        return (data) => method.Invoke(_instanceCreator.CreateInstance(controller, types, Types), data);
    }
    private string GetPathForInstance(Type controller, MethodInfo method)
    {
        var controllerName = GetControllerName(controller);
        var methodName = method.Name;
        AddControllerMethods(controllerName, methodName, method);

        if (controllerName.EndsWith("controller", StringComparison.InvariantCultureIgnoreCase))
        {
            controllerName = controllerName[..^"controller".Length];
        }
        var attribute = method.GetCustomAttributes<HttpGetAttribute>().FirstOrDefault();

        var pathSuffix = attribute?.Path ?? methodName;

        return method.Name.Equals("Index", StringComparison.InvariantCultureIgnoreCase) ? "/" + controllerName : $"/{controllerName}/{pathSuffix}".TrimEnd('/');
    }
    private string GetControllerName(Type controller)
    {
        var name = controller.Name;
        if (name.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase))
        {
            name = name[..^"Controller".Length];
        }

        return name;
    }
    private void AddControllerMethods(string controllerName, string methodName, MethodInfo method)
    {
        if (!ControllerMethods.ContainsKey(controllerName))
        {
            _controllerMethods.Add(controllerName, new Dictionary<string, MethodInfo>());
        }
        if (ControllerMethods.TryGetValue(controllerName, out Dictionary<string, MethodInfo>? methods))
        {
            methods.Add(methodName, method);
        }
    }
    #endregion


    public void AddTransient<TInterface, TType>() where TInterface : class where TType : class
    {
        AddTransient(typeof(TInterface), typeof(TType));
    }

    public void AddTransient(Type typeInterface, Type type)
    {
        Types.Add(typeInterface, type);
    }

    private Dictionary<string, Func<object[], object?>> CreateRouteDictionary(Assembly controllersAssembly)
    {
        return controllersAssembly.GetTypes()
            .Where(type => typeof(IController).IsAssignableFrom(type))
            .SelectMany(Controller => Controller.GetMethods().Where(method => method.DeclaringType != typeof(object))
            .Select(Method => new
            {
                Controller,
                Method
            }))
            .ToDictionary(
            key => GetPathForInstance(key.Controller, key.Method),
            value => GetEndpoint(value.Controller, value.Method)
            );
    }

}