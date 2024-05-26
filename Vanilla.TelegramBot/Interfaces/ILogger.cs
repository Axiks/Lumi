using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
