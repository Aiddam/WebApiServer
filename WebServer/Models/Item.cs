using WebServer.Inter;

namespace WebServer.Models
{
    public class Item : IItem
    {
        public Item(int b, string c)
        {
            Write();
        }

        public void Write()
        {
            Console.WriteLine("Item");
        }
    }
}
