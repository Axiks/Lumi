using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Common.Enums;

namespace Vanilla_App.Models
{
    public class ProjectModel
    {
        public Guid Id { get; set; }
        public Guid OwnerId {  get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required DevelopmentStatusEnum DevelopmentStatus { get; set; }
        public IEnumerable<string> Links { get; set; } = Enumerable.Empty<string>();
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
