using Server.Models.Enum;
using System.Linq.Expressions;
using System.Reflection;

namespace Server.Extractors
{
    public class InstanceCreator
    {
        private readonly Dictionary<Type, Func<object>> _activators = new();
        private readonly Dictionary<Type, ConstructorInfo> _constructorCache = new();

        /// <summary>
        /// Dynamically creates a class using expressions
        /// </summary>
        /// <param name="type">Type of object to be created</param>
        /// <param name="arguments">Parameters to pass to the class constructor</param>
        /// <param name="lifetimeScopedTypes">List of dependencies that have been transferred</param>
        /// <returns></returns>
        public object CreateInstance(Type type, Type[]? arguments, Dictionary<Type, Tuple<LifeTime, Type>> lifetimeScopedTypes)
        {
            if (!_activators.TryGetValue(type, out Func<object>? activator))
            {
                activator = CreateActivator(type, arguments, lifetimeScopedTypes);
                _activators[type] = activator!;
            }

            return activator!();
        }

        private Func<object>? CreateActivator(Type type, Type[]? arguments, Dictionary<Type, Tuple<LifeTime, Type>> lifetimeScopedTypes)
        {
            ConstructorInfo constructor = GetConstructorInfo(type);
            List<Expression> expressionList = GetExpressions(arguments, lifetimeScopedTypes);

            var lambda = Expression.Lambda<Func<object>>(Expression.New(constructor, expressionList));
            return lambda.Compile();
        }
        private List<Expression> GetExpressions(Type[]? arguments, Dictionary<Type, Tuple<LifeTime, Type>> types)
        {
            List<Expression> expressionList = new(arguments!.Length);
            for (int arg = 0; arg < arguments.Length; arg++)
            {
                if (types.TryGetValue(arguments[arg], out var value))
                {
                    Type[] parameters = GetParameters(value.Item2);
                    ConstructorInfo construct = GetConstructorInfo(value.Item2, parameters);
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
            if (!_constructorCache.TryGetValue(type, out ConstructorInfo? constructor))
            {
                Type[] parameters = GetParameters(type);
                constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, parameters, null)!;
                _constructorCache[type] = constructor;
            }

            return constructor;
        }

        private ConstructorInfo GetConstructorInfo(Type type, Type[] parameters)
        {
            return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, parameters)!;
        }
        private Type[] GetParameters(Type type)
        {
            var constructor = type.GetConstructors().FirstOrDefault();
            if (constructor != null)
            {
                return constructor.GetParameters().Select(param => param.ParameterType).ToArray();
            }
            return Array.Empty<Type>();
        }
    }
}
