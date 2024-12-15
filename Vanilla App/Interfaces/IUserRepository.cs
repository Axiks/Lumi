using Vanilla.Data.Entities;
using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    public interface IUserRepository
    {
        public Task<List<ProjectModel>> GetProjectsAsync(Guid userId);
        public List<UserEntity> GetAll();
        public UserEntity Get(Guid userId);
        public UserEntity Create(UserCreateRequestModel update);
        public UserEntity Update(Guid userId, UserUpdateRequestModel update);
        public void Delete (Guid userId);
    }
}
