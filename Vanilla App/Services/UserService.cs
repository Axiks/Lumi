using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Data;
using Vanilla.OAuth.Entities;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using Vanilla_App.Repository;

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
