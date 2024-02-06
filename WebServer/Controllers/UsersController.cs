using Server.Attributes;
using Server.Interfaces.HandlersAndControllers;
using WebServer.Models;
using WebServer.Repositories;

namespace WebServer.Controllers
{
    public class UsersController : IController
    {
        private readonly IUserRepository _userRepository;
        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public ICollection<User> Index()
        {
            return _userRepository.GetUsers();
        }
        public Task<ICollection<User>> IndexAsync()
        {
            return Task.Run(() =>
            {
                ICollection<User> users = _userRepository.GetUsers();
                return users;
            });
        }
        [HttpPost]
        public void AddUser(User user)
        {
            _userRepository.AddUser(user);
        }

        [HttpGet]
        public Task<User>? GetUser(int id)
        {
            return Task.Run(() =>
            {
                ICollection<User> users = _userRepository.GetUsers();
                return users.FirstOrDefault(u => u.Id == id);
            });
        }
    }
}
