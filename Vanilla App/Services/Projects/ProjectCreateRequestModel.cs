using Vanilla.Common.Enums;

namespace Vanilla_App.Services.Projects
{
    public class ProjectCreateRequestModel
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required DevelopmentStatusEnum DevelopStatus { get; set; }
        public IEnumerable<string> Links { get; set; }
    }
}
