using Server.Attributes;
using Server.Interfaces.HandlersAndControllers;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class MainController : IController
    {
        private static List<Item> items = new List<Item>();
        public string Index()
        {
            return "Get Index";
        }
        [HttpGet]
        public ICollection<Item> GetItems()
        {
            return items;
        }

        [HttpPost]
        public void AddItem(Item item)
        {
            items.Add(item);
        }
    }
}
