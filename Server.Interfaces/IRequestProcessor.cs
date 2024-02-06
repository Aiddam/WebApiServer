
using System.Reflection;

namespace Server.Interfaces
{
    public interface IRequestProcessor
    {
        object[] ProcessRequest(Request request, MethodInfo method);
    }
}
