using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Data;
using Vanilla_App.Helpers;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;

namespace Vanilla_App.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ProjectModel>> GetProjectsAsync(Guid userId)
        {
            List<ProjectModel> userProjexts = new List<ProjectModel>();
            var userProjextsEntity = await _dbContext.Projects.Where(x => x.OwnerId == userId).ToListAsync();
            foreach (var project in userProjextsEntity)
            {
                userProjexts.Add(MapperHelper.ProjectEntityToProjectModel(project));
            }

            return userProjexts;
        }
    }
}
