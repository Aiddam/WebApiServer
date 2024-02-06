
namespace Server.Attributes
{
    public abstract class BaseHttpMethodAttribute : Attribute
    {
        public string Path { get; init; }
    }
}
