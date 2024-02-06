using Server.Interfaces;
using System.Reflection;

namespace Server.Route.RequestProccessors
{
    public abstract class BaseRequestProcessor: IRequestProcessor
    {
        public abstract object[] ProcessRequest(Request request, MethodInfo method);
        protected object ParseParameter(ParameterInfo parameterInfo, object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value), $"Value for parameter {parameterInfo.Name} is null.");
            return parameterInfo.ParameterType switch
            {
                Type t when t == typeof(int) => ParseIntParameter(value),
                Type t when t == typeof(string) => value as string ?? string.Empty,
                _ => throw new NotSupportedException($"Unsupported parameter type: {parameterInfo.ParameterType}")
            };
        }
        private static int ParseIntParameter(object value)
        {
            if (int.TryParse(value as string, out int intValue))
            {
                return intValue;
            }
            throw new ArgumentException("Invalid integer value.", nameof(value));
        }
    }
}
