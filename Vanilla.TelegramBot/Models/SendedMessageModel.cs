using Vanilla.Common.Enums;

namespace Vanilla.TelegramBot.Models
{
    public record SendedMessageModel(int messageId, DeleteMessageMethodEnum method);
}
