namespace Vanilla_App.Services.Projects.Repository
{
    public interface IProjectRepository
    {
        public Task<List<ProjectModel>> GetAllAsync();
        public Task<ProjectModel> GetAsync(Guid projectId);
        public Task<ProjectModel> CreateAsync(Guid ownerId, ProjectCreateRequestModel projectRequest);
        public Task<ProjectModel> UpdateAsync(ProjectUpdateRequestModel project);
        public void Delete(Guid projectId);
    }
}
