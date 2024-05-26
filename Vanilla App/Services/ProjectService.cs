using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Data;
using Vanilla.Data.Entities;
using Vanilla.OAuth.Entities;
using Vanilla_App.Helpers;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using Vanilla_App.Repository;

namespace Vanilla_App.Services
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
