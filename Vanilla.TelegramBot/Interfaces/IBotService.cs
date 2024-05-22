using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.TelegramBot.Interfaces
{
    public interface IBotService
    {
        public Task StartListening();
    }
}
