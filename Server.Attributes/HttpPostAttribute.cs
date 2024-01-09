
namespace Server.Attributes
{
    public class HttpPostAttribute: BaseHttpMethodAttribute
    {
        public string Path { get; init; }
        public HttpPostAttribute(){}
        public HttpPostAttribute(string path)
        {
            Path = path;
        }
    }
}
