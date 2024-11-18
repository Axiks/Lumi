using Microsoft.EntityFrameworkCore;
using Vanilla.Common.Enums;
using Vanilla.Data;
using Vanilla.Data.Entities;
using Vanilla_App.Helpers;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;


namespace Vanilla_App.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public ProjectRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ProjectModel> CreateAsync(Guid ownerId, ProjectCreateRequestModel projectRequest)
        {
            var projectEntity = new ProjectEntity { Name = projectRequest.Name, Description = projectRequest.Description, DevelopStatus = projectRequest.DevelopStatus, OwnerId = ownerId };

            if(projectRequest.Links is not null)
            {
                foreach (var url in projectRequest.Links)
                {
                    projectEntity.Links.Add(new LinkEntity { Url = url });
                }
            }


            await _dbContext.AddAsync(projectEntity);
            await _dbContext.SaveChangesAsync();

            var projectModel = MapperHelper.ProjectEntityToProjectModel(projectEntity);

            return projectModel;
        }

        public void Delete(Guid projectId)
        {
            var project = _dbContext.Projects
                .Include(x => x.Links)
                .FirstOrDefault(x => x.Id == projectId);
            project.Links.RemoveAll(x => x.Id == projectId);
            _dbContext.Projects.Remove(project);
            _dbContext.SaveChanges();
        }

        public async Task<List<ProjectModel>> GetAllAsync()
        {
            List<ProjectModel> allProjects = new List<ProjectModel>();
            var allProjectsEntity = await _dbContext.Projects
                .Include(x => x.Links)
                .ToListAsync();
            foreach (var projectEntity in allProjectsEntity)
            {
                allProjects.Add(MapperHelper.ProjectEntityToProjectModel(projectEntity));
            }

            return allProjects;
        }

        public async Task<ProjectModel> GetAsync(Guid projectId)
        {
            var projectEntity = await _dbContext.Projects.FirstAsync(x => x.Id == projectId);
            return MapperHelper.ProjectEntityToProjectModel(projectEntity);
        }

        public async Task<ProjectModel> UpdateAsync(ProjectUpdateRequestModel project)
        {
            var projectEntity = await _dbContext.Projects.FirstAsync(x => x.Id == project.Id);
            if (project.Name != null) projectEntity.Name = project.Name;
            if (project.Description != null) projectEntity.Description = project.Description;

            if (project.ProjectRequest is not null) projectEntity.DevelopStatus = (DevelopmentStatusEnum)project.ProjectRequest;

            // erse urls and fetch new
            if (project.Links != null)
            {
                projectEntity.Links = new List<LinkEntity>();
                foreach (var url in project.Links)
                {
                    projectEntity.Links.Add(new LinkEntity { Url = url });
                }
            }

            await _dbContext.SaveChangesAsync();

            return MapperHelper.ProjectEntityToProjectModel(projectEntity);
        }
    }
}