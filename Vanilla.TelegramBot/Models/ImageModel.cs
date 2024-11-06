using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.TelegramBot.Models
{
    public class ImageModel
    {
        public required string TgMediaId { get; init; }
        public string TgUrl { get; set; }
/*        public int Width { get; set; }
        public int Height { get; set; }
        public int Size { get; set; }*/
    }
}
