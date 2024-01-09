using Server.Interfaces;
using System.Reflection;

namespace Server.Route.RequestProccessors
{
    public abstract class BaseRequestProcessor: IRequestProcessor
    {
        public abstract object[] ProcessRequest(Request request, MethodInfo method);
        protected object ParseParameter(ParameterInfo parameterInfo, object value)
        {
            return parameterInfo.ParameterType switch
            {
                Type t when t == typeof(int) => int.TryParse(value as string, out int intValue) ? intValue : default,
                Type t when t == typeof(string) => value,
                _ => null!
            };
        }
    }
}
