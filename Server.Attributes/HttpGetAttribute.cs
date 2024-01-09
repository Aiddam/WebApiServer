
namespace Server.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class HttpGetAttribute : BaseHttpMethodAttribute
    {
        public string Path { get; init; }
        public string MethodName { get; init; }
        public string[] MethodParameters { get; init; }
        public HttpGetAttribute(){}
        public HttpGetAttribute(string path)
        {
            Path = path;
            MethodName = path.Split("/").First();
            MethodParameters = path.Split("/").Skip(1).ToArray();
        }
    }
}
