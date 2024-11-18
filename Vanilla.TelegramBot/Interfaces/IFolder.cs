using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Services.Bot;

namespace Vanilla.TelegramBot.Interfaces
{

    public delegate void CloseFolderEventHandler();
    public interface IFolder
    {
        public void Run();
        public void EnterPoint(Update update);
        //public void NextPage();
        //internal void PreviousPage();
        //public void GoToPage(string path);
        public void CloseFolder();
        //internal short CurrentPageIndex { get; }
        //internal short NumberOfPages {  get; }

        public event CloseFolderEventHandler? CloseFolderEvent;
    }
}
