using Vanilla.Data.Entities;
using Vanilla_App.Services.Projects;

namespace Vanilla_App.Services.Users.Repository
{
    public interface IUserRepository
    {
        public Task<List<ProjectModel>> GetProjectsAsync(Guid userId);
        public List<UserEntity> GetAll();
        public UserEntity Get(Guid userId);
        public UserEntity Create(CoreUserCreateRequestModel update);
        public UserEntity Update(Guid userId, CoreUserUpdateRequestModel update);
        public void Delete(Guid userId);
    }
}
