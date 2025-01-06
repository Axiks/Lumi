using Vanilla_App.Services.Projects.Repository;

namespace Vanilla_App.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectModel> ProjectCreateAsync(Guid userId, ProjectCreateRequestModel project)
        {
            return await _projectRepository.CreateAsync(userId, project);
        }

        public void ProjectDelete(Guid projectId)
        {
            _projectRepository.Delete(projectId);
        }

        public async Task<List<ProjectModel>> ProjectGetAllAsync()
        {
            return await _projectRepository.GetAllAsync();
        }

        public async Task<ProjectModel> ProjectGetAsync(Guid projectId)
        {
            return await _projectRepository.GetAsync(projectId);
        }

        public async Task<ProjectModel> ProjectUpdateAsync(ProjectUpdateRequestModel project)
        {
            return await _projectRepository.UpdateAsync(project);
        }
    }
}
