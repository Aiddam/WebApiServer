using WebServer.Models;

namespace WebServer.Repositories
{
    public interface IUserRepository
    {
        User[] GetUsers();
    }
    public class UserRepository : IUserRepository
    {
        public int MyProperty { get; set; }
        public User[] GetUsers()
        {
            return new User[]
            {
                new User("Anton", "Sharlai", 19),
                new User("Evgeniy", "Zhuravel", 19),
                new User("Alex", "Tyrchin", 19),
                new User("Dima", "Gorbotenko", 19),
                new User("Pavel", "Archipov", 20)
            };
        }
    }
}
