using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    public interface IUserRepository
    {
        public Task<List<ProjectModel>> GetProjectsAsync(Guid userId);
    }
}
