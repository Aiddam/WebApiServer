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
                try
                {
                    data[i] = ParseParameter(parameters[i], request.Data[i]);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error processing parameter {parameters[i].Name}: {ex.Message}", ex);
                }
            }

            return data;
        }
    }
}
