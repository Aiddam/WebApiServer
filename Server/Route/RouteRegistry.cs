using Server.Attributes;
using Server.Extractors;
using Server.Interfaces.HandlersAndControllers;
using System.Reflection;

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
    private string GetPath(Type controller, MethodInfo method)
    {
        string name = controller.Name;
        IEnumerable<Attribute> MethodAtributtes = method.GetCustomAttributes();
        if (!ControllerMethods.ContainsKey(controller.Name))
        {
            _controllerMethods.Add(controller.Name, new Dictionary<string, MethodInfo>());
        }
        if (ControllerMethods.TryGetValue(controller.Name, out Dictionary<string, MethodInfo>? methods))
        {
            methods.Add(method.Name, method);
        }

        if (name.EndsWith("controller", StringComparison.InvariantCultureIgnoreCase))
        {
            name = name[..^"controller".Length];
        }
        if (MethodAtributtes.Any(m => m.GetType() == typeof(HttpGetAttribute)))
        {
            HttpGetAttribute? getAttribute = method.GetCustomAttribute<HttpGetAttribute>();

            return name + "/" + getAttribute?.Path;
        }
        return method.Name.Equals("Index", StringComparison.InvariantCultureIgnoreCase) ? "/" + name : "/" + name + "/" + method.Name;
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
            key => GetPath(key.Controller, key.Method),
            value => GetEndpoint(value.Controller, value.Method)
            );
    }

}