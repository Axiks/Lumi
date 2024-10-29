using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Interfaces
{
    public interface ILogger
    {
        public Guid WriteLog(string message, LogType logType);
        public List<LogModel> ReadLogs();
    }
}
