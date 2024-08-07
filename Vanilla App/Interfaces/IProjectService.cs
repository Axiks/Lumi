﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.OAuth.Entities;
using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
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
