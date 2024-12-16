namespace Vanilla_App.Services.Projects
{
    public interface IProjectService
    {
        public Task<List<ProjectModel>> ProjectGetAllAsync();
        public Task<ProjectModel> ProjectGetAsync(Guid projectId);
        public Task<ProjectModel> ProjectCreateAsync(Guid userId, ProjectCreateRequestModel project);
        public Task<ProjectModel> ProjectUpdateAsync(ProjectUpdateRequestModel project);
        public void ProjectDelete(Guid projectId);
    }
}
