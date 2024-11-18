using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Services
{
    public class ConsoleLoggerService : ILogger
    {
        private List<LogModel> _loggs = new List<LogModel>();
        public List<LogModel> ReadLogs() => _loggs;

        public Guid WriteLog(string message, LogType logType)
        {

            string logFolderPath = "AppLog";
            Directory.CreateDirectory(logFolderPath);


            var log = new LogModel
            {
                Message = message,
                LogType = logType,
                Id = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
            };
            _loggs.Add(log);

            switch (logType)
            {
                case LogType.Information:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.WriteLine("t: " + DateTime.UtcNow + " & " + logType + " :3 " + message);
            Console.ForegroundColor = ConsoleColor.White;

            using (StreamWriter sw = File.AppendText(logFolderPath + "/" + "log.txt"))
            {
                sw.WriteLine(log.Id + " - " + logType + " & " + message);
            }

            return log.Id;
        }
    }
}
