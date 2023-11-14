using WebServer.Inter;

namespace WebServer.Models
{
    public class Item : IItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Item(int b, string c)
        {
            Write();
        }
        public Item()
        {
            
        }

        public void Write()
        {
            Console.WriteLine("Item");
        }
    }
}
