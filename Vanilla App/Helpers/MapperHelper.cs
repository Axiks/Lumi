using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Data.Entities;
using Vanilla_App.Models;

namespace Vanilla_App.Helpers
{
    public static class MapperHelper
    {
        public static ProjectModel ProjectEntityToProjectModel(ProjectEntity projectEntity)
        {
            var projectModel = new ProjectModel
            {
                Id = projectEntity.Id,
                Name = projectEntity.Name,
                Description = projectEntity.Description,
                OwnerId = projectEntity.OwnerId,
                Links = projectEntity.Links.Select(x => x.Url),
                Created = projectEntity.Created,
                Updated = projectEntity.Update
            };
            return projectModel;
        }
    }
}
