using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Services
{
    public class ConsoleLogger : ILogger
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
            Console.WriteLine(logType + " & " + message);
            Console.ForegroundColor = ConsoleColor.White;

            using (StreamWriter sw = File.AppendText(logFolderPath + "/" + "log.txt"))
            {
                sw.WriteLine(logType + " & " + message);
            }

            return log.Id;
        }
    }
}
