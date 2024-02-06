using Server.Attributes;
using WebServer.Models;

namespace WebServer.Repositories
{
    public interface IUserRepository
    {
        ICollection<User> GetUsers();
        void AddUser(User user);
    }
    public class UserRepository : IUserRepository
    {
        private static IList<User> _users = new List<User>();
        [HttpPost]
        public void AddUser(User user)
        {
            _users.Add(user);
        }

        [HttpGet]
        public ICollection<User> GetUsers()
        {
            return _users;
        }
    }
}
