using Server.Models;

namespace Server.Interfaces
{
    public interface IMiddleware
    {
        Task InvokeAsync(ServerContext context, Func<Task> next);
    }
}
