using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla_App.Models
{
    public class ProjectUpdateRequestModel
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public IEnumerable<string> Links { get; set; }
    }
}
