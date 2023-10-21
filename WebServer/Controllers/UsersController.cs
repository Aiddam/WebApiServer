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
        public User[] Index()
        {
            return _userRepository.GetUsers();
        }
        public Task<User[]> IndexAsync()
        {
            return Task.Run(() =>
            {
                User[] users = _userRepository.GetUsers();
                return users;
            });
        }

        [HttpGet("User/{id}")]
        public Task<User>? GetUser(int id)
        {
            return Task.Run(() =>
            {
                User[] users = _userRepository.GetUsers();
                return users.FirstOrDefault(u => u.Id == id);
            });
        }
    }
}
