using Server.Interfaces.HandlersAndControllers;

namespace WebServer.Controllers
{
    public class MainController : IController
    {
        public string Index()
        {
            return "Get Index";
        }
    }
}
