using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.TelegramBot.Helpers;

namespace Vanilla.TelegramBot.Entityes
{
    public class ImagesEntity
    {
        public Guid Id { get; set; }
        public Guid CoreId { get; set; }
        public required string TgMediaId { get; init; }
    }
}
