using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.TelegramBot.Models
{
    public class BotUpdateProjectModel
    {
        public readonly Guid UserId;
        public readonly Guid ProjectId;
        public readonly SelectedItem SelectedItem;

        public readonly string InitMessageId;
        public BotUpdateProjectModel(Guid userId, Guid projectId, SelectedItem selectedItem, string initMessageId)
        {
            UserId = userId;
            ProjectId = projectId;
            SelectedItem = selectedItem;
            InitMessageId = initMessageId;
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
