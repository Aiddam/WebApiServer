using Server.Interfaces;
using Server.Models;

namespace WebServer.Middlewares
{
    public class ExceptionHandler : IMiddleware
    {
        public async Task InvokeAsync(ServerContext context, Func<Task> next)
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.Message}");
            }
        }
    }
}
