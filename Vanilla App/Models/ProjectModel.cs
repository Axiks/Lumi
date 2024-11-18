using Vanilla.Common.Enums;

namespace Vanilla_App.Models
{
    public class ProjectModel
    {
        public required Guid Id { get; init; }
        public required Guid OwnerId { get; init; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required DevelopmentStatusEnum DevelopmentStatus { get; set; }
        public IEnumerable<string> Links { get; set; } = Enumerable.Empty<string>();
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
