using Server.Models;

namespace Server.Delegates
{
    public delegate Task MiddlewareDelegate(ServerContext context, Func<Task> next);
}
