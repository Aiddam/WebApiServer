
namespace Server.Attributes
{
    public class HttpPostAttribute: BaseHttpMethodAttribute
    {
        public HttpPostAttribute(){}
        public HttpPostAttribute(string path)
        {
            Path = path;
        }
    }
}
