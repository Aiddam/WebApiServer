namespace Server.Interfaces.HandlersAndControllers
{
    public interface IHandler
    {
        Task HandleAsync(Stream networkStream, Request request);
    }
}
