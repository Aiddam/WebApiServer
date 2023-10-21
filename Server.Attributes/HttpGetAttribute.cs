
namespace Server.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class HttpGetAttribute : Attribute
    {
        public string Path { get; private set; }
        public string MethodName { get; private set; }
        public string[] MethodParameters { get; private set; }
        public HttpGetAttribute(string path)
        {
            Path = path;
            MethodName = path.Split("/").First();
            MethodParameters = path.Split("/").Skip(1).ToArray();
        }
    }
}
