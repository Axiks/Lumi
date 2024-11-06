using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.TelegramBot.Entityes
{
    public class ImagesEntity
    {
        public Guid Id { get; set; }
        public required string TgMediaId { get; init; }
        public required string TgUrl { get; init; }
    }
}
