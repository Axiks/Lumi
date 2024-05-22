using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    public interface IUserRepository
    {
        public Task<List<ProjectModel>> GetProjectsAsync(Guid userId);
    }
}
