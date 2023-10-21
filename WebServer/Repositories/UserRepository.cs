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
                new User("Anton", "Sharlai", 19, 1),
                new User("Evgeniy", "Zhuravel", 19, 2),
                new User("Alex", "Tyrchin", 19, 3),
                new User("Dima", "Gorbotenko", 19, 4),
                new User("Pavel", "Archipov", 20, 5)
            };
        }
    }
}
