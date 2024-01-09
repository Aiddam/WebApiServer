using Server.Interfaces;
using Server.Interfaces.HandlersAndControllers;
using Server.Models;
using System.IO;

namespace Server.Middlewares
{
    public class FinalMiddleware : IMiddleware
    {
        private readonly IHandler _handler;
        public FinalMiddleware(IHandler handler)
        {
            _handler = handler;
        }

        public async Task InvokeAsync(ServerContext context, Func<Task> next)
        {
            await _handler.HandleAsync(context.Stream, context.Request);
        }
    }
}
