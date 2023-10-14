using System.Linq.Expressions;
using System.Reflection;

namespace Server.Extractors
{
    public class InstanceCreator
    {
        private readonly Dictionary<Type, Func<object>> _activators = new();

        public object CreateInstance(Type type, Type[]? arguments, Dictionary<Type, Type> types)
        {
            if (!_activators.TryGetValue(type, out Func<object>? val))
            {
                _activators.Add(type, val = CreateActivator(type, arguments, types)!);
            }

            return val();
        }

        private Func<object>? CreateActivator(Type type, Type[]? arguments, Dictionary<Type, Type> types)
        {
            Type[] parameters = GetParameters(type);
            ConstructorInfo constructor = GetConstructorInfo(type, parameters);
            if (arguments?.Length > 0)
            {

                List<Expression> expressionList = GetExpressions(parameters, types);

                return (Func<object>)Expression.Lambda(typeof(Func<object>),
                    Expression.New(constructor!, expressionList))
                .Compile();
            }
            return (Func<object>)Expression.Lambda(typeof(Func<object>),
                                                   Expression.New(constructor!))
                                 .Compile();
        }
        private List<Expression> GetExpressions(Type[]? arguments, Dictionary<Type, Type> types)
        {
            List<Expression> expressionList = new(arguments!.Length);
            for (int arg = 0; arg < arguments.Length; arg++)
            {
                if (types.TryGetValue(arguments[arg], out Type? value))
                {
                    Type[] parameters = GetParameters(value);
                    ConstructorInfo construct = GetConstructorInfo(value, parameters);
                    expressionList.Add(Expression.New(construct, GetExpressions(parameters, types)));
                }
                if (arguments[arg].IsPrimitive)
                {
                    expressionList.Add(Expression.New(arguments[arg]));
                }
                if (arguments[arg].Name == "String" || arguments[arg].IsArray)
                {
                    Type[] parameters = GetParameters(arguments[arg]);
                    ConstructorInfo construct = GetConstructorInfo(arguments[arg], parameters);
                    expressionList.Add(Expression.New(construct, GetExpressions(parameters, types)));
                }
            }
            return expressionList;
        }

        private ConstructorInfo GetConstructorInfo(Type type)
        {
            Type[] parameters = GetParameters(type);
            return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, parameters)!;
        }

        private ConstructorInfo GetConstructorInfo(Type type, Type[] parameters)
        {
            return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, parameters)!;
        }
        private Type[] GetParameters(Type type)
        {
            ParameterInfo[]? parameters = type.GetConstructors()?.FirstOrDefault()?.GetParameters();
            Type[] types = new Type[parameters!.Length];
            for (int parametr = 0; parametr < parameters.Length; parametr++)
            {
                types[parametr] = parameters[parametr].ParameterType;
            }
            return types;
        }

    }
}
