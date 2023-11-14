
namespace Server.Attributes
{
    public class HttpPostAttribute: Attribute
    {
        public string Path { get; init; }
        public HttpPostAttribute()
        {
            
        }
        public HttpPostAttribute(string path)
        {
            Path = path;
        }
    }
}
