namespace Vanilla.TelegramBot.Models
{
    public class BotUpdateProjectModel
    {
        public readonly Guid ProjectId;
        public SelectedItem? SelectedItem;

        public readonly string InitMessageId;
        public BotUpdateProjectModel(Guid projectId)
        {
            ProjectId = projectId;
        }
    }

    public enum SelectedItem
    {
        Name,
        Description,
        Status,
        Links
    }
}
