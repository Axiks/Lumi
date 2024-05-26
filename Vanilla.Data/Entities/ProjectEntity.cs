using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Common.Enums;

namespace Vanilla.Data.Entities
{
    public class ProjectEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Guid OwnerId {  get; set; }
        public required DevelopmentStatusEnum DevelopStatus { get; set; }
        public List<LinkEntity> Links { get; set; } = new List<LinkEntity>();
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Update { get; set; } = DateTime.UtcNow;

    }
}
