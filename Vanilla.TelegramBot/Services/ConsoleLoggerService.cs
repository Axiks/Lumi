using System.Text.Json;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Services
{
    public class ConsoleLoggerService : ILogger
    {
        List<LogModel> _loggs = new List<LogModel>();

        string _logFolderPath = "AppLog";
        public List<LogModel> ReadLogs() => TakeAllLogs();

        public Guid WriteLog(string message, LogType logType)
        {
            var log = new LogModel
            {
                Message = message,
                LogType = logType,
                Id = Guid.NewGuid(),
                CreateAt = DateTime.UtcNow,
            };
            _loggs.Add(log);

            WriteLogToConsole(log);
            WriteLogToFile(log);

            return log.Id;
        }

        void WriteLogToConsole(LogModel log)
        {
            switch (log.LogType)
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
            Console.WriteLine(MakeLogStringHelper(log));
            Console.ForegroundColor = ConsoleColor.White;
        }

        void WriteLogToFile(LogModel log)
        {
            Directory.CreateDirectory(_logFolderPath);

            using (StreamWriter sw = System.IO.File.AppendText(_logFolderPath + "/" + "log.txt"))
            {
                sw.WriteLine(MakeLogStringHelper(log));
            }
        }

        List<LogModel> TakeAllLogs()
        {
            var logs = new List<LogModel>();

            foreach (var line in File.ReadLines(_logFolderPath + "/" + "log.txt"))
            {
                logs.Add(DeserialiseLogToStringLine(line));
            }

            return logs;
        }

        string SerialiseLogToStringLine(LogModel log) => MakeLogStringHelper(log);
        LogModel DeserialiseLogToStringLine(string logString)
        {
            string[] parts = logString.Split(new string[] { " & ", " :3 ", " t: " }, StringSplitOptions.None);
            return new LogModel() { Id = Guid.Parse(parts[0]), LogType = (LogType) Enum.Parse(typeof(LogType), parts[1]), Message = parts[2], CreateAt = DateTime.Parse(parts[3]) };
        }

        //string MakeLogStringHelper(LogModel log) => String.Format() log.Id.ToString() + " & " + log.LogType + " :3 " + log.Message + "t: " + log.CreateAt.ToString();
        string MakeLogStringHelper(LogModel log) => String.Format("{0} & {1} :3 {2} t: {3}", log.Id, log.LogType, log.Message, log.CreateAt);
    }
}
