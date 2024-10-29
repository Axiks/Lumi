using Vanilla_App.Interfaces;
using Vanilla_App.Models;

namespace Vanilla_App.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<ProjectModel>> UserProjectsGetAsync(UserModel user)
        {
            return await _userRepository.GetProjectsAsync(user.Id);
        }
    }
}
