using System.Runtime.CompilerServices;
using System.Text.Json;
using Telegram.BotAPI.AvailableTypes;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Services
{
    public class ConsoleLoggerService(IUserService userService) : Vanilla.TelegramBot.Interfaces.ILogger
    {
        string _logFolderPath = "AppLog";
        public List<LogModel> ReadLogs() => TakeAllLogs();

        public Guid WriteLog(string message,
            LogType logType, Guid? UserId = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
            )
        {
            var log = new LogModel
            {
                Id = Guid.NewGuid(),
                Message = message,
                LogType = logType,
                MemberName = memberName,
                FilePath = sourceFilePath,
                LineNumber = sourceLineNumber,
                CreateAt = DateTime.UtcNow,
            };
            if(UserId != null) log.UserId = UserId;

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
                sw.WriteLine(SerialiseLogToStringLine(log));
            }
        }

        List<LogModel> TakeAllLogs()
        {
            var logs = new List<LogModel>();

            foreach (var line in System.IO.File.ReadLines(_logFolderPath + "/" + "log.txt"))
            {
                logs.Add(DeserialiseLogToStringLine(line));
            }

            return logs;
        }

        string SerialiseLogToStringLine(LogModel log) => String.Format("{0} & {1} :3 {2} t: {3} u: {4}, m: {5}, p: {6}, l: {7}",
                log.Id, log.LogType, log.Message, log.CreateAt, log.UserId, log.MemberName, log.FilePath, log.LineNumber);
        LogModel DeserialiseLogToStringLine(string logString)
        {
            string[] parts = logString.Split(new string[] { " & ", " :3 ", " t: ", " u: " }, StringSplitOptions.None);
            return new LogModel() { 
                Id = Guid.Parse(parts[0]),
                LogType = (LogType) Enum.Parse(typeof(LogType), parts[1]),
                Message = parts[2],
                CreateAt = DateTime.Parse(parts[3]),
                UserId = Guid.Parse(parts[4]),
                MemberName = parts[5],
                FilePath = parts[6],
                LineNumber = Int32.Parse(parts[7]),
            };
        }

        string MakeLogStringHelper(LogModel log)
        {
            UserModel? user = log.UserId is not null ? userService.GetUser((Guid)log.UserId).Result : null;
            var userName = user is not null ? user.Username ?? user.TelegramId.ToString() : "";

            return String.Format("{0} & {1} :3 {2} t: {3} user: {4}, m: {5}, p: {6}, l: {7}",
                log.Id, log.LogType, log.Message, log.CreateAt, userName, log.MemberName, log.FilePath, log.LineNumber);
        }

    }
}
