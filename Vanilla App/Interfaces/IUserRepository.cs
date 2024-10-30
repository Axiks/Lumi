using Vanilla.Data.Entities;
using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    public interface IUserRepository
    {
        public Task<List<ProjectModel>> GetProjectsAsync(Guid userId);
        public UserEntity Get(Guid userId);
        public UserEntity Create(UserCreateRequestModel update);
        public UserEntity Update(Guid userId, UserUpdateRequestModel update);
    }
}
