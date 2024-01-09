using System.Reflection;

namespace Server.Route.RequestProccessors
{
    public class HttpGetRequestProcessor : BaseRequestProcessor
    {
        public override object[] ProcessRequest(Request request, MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] data = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                data[i] = ParseParameter(parameters[i], request?.Data?[i]!);
            }

            return data;
        }
    }
}
