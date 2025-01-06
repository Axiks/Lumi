using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Interfaces
{
    public interface ILogger
    {
        public Guid WriteLog(string message,
            LogType logType,
            Guid? UserId = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
            );
        public List<LogModel> ReadLogs();
    }
}
