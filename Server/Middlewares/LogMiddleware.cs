using Server.Interfaces;
using Server.Models;

namespace WebServer.Middlewares
{
    public class LogMiddleware : IMiddleware
    {
        public async Task InvokeAsync(ServerContext context, Func<Task> next)
        {
            var request = context.Request;
            if (!String.IsNullOrEmpty(request.Path))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                await Console.Out.WriteLineAsync($"{request.HttpMethod}: {request.MethodName} - {request.Path}");
                Console.ResetColor();
            }
            await next();
        }
    }
}
