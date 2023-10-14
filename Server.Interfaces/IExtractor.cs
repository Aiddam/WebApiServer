
namespace Server.Interfaces
{
    public interface IExtractor<T>
    {
        object? ExtractValue(T input);
    }
}
