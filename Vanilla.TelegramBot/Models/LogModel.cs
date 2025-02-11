﻿using Vanilla.Common.Enums;

namespace Vanilla.TelegramBot.Models
{
    public class LogModel
    {
        public required Guid Id { get; set; }
        public required string Message { get; set; }
        public string? MemberName { get; set; }
        public string? FilePath { get; set; }
        public int? LineNumber { get; set; }
        public Guid? UserId { get; set; }
        public required LogType LogType { get; set; }
        public required DateTime CreateAt { get; set; }
    }
}
