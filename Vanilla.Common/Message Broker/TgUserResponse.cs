using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.Common.Message_Broker
{
    public class TgUserResponse
    {
        public required Guid UserId { get; init; }
        public required long TgId { get; init; }
        public string? Username { get; set; }
        public List<string> ImagesId { get; set; }
    }
}
