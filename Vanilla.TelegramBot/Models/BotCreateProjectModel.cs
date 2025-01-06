using Vanilla.Common.Enums;

namespace Vanilla.TelegramBot.Models
{
    public class BotCreateProjectModel
    {
        public readonly Guid UserId;
        public readonly long TelegramUserId;
        public String? Name { get; set; }
        public String? Description { get; set; }
        public DevelopmentStatusEnum? DevelopmentStatus { get; set; }
        public string PollIdDevelopmentStatus { get; set; }
        public List<string>? Links { get; set; }
        public List<int> SendedMessages { get; set; } = new List<int>();

        public BotCreateProjectModel(Guid UserId, long TelegramUserId)
        {
            this.UserId = UserId;
            this.TelegramUserId = TelegramUserId;
        }
    }
}
