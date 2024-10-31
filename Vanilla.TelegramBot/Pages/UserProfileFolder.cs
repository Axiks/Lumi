using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.TelegramBot.Interfaces;

namespace Vanilla.TelegramBot.Pages
{
    internal class UserProfileFolder : IFolder
    {
        short IFolder.CurrentPageIndex => throw new NotImplementedException();

        short IFolder.NumberOfPages => throw new NotImplementedException();

        event CloseFolderEventHandler IFolder.CloseFolderEvent
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public void NextPage()
        {
            throw new NotImplementedException();
        }

        void IFolder.CloseFolder()
        {
            throw new NotImplementedException();
        }

        void IFolder.EnterPoint(Update update)
        {
            throw new NotImplementedException();
        }

        void IFolder.GoToPage(short index)
        {
            throw new NotImplementedException();
        }

        void IFolder.NextPage()
        {
            throw new NotImplementedException();
        }

        void IFolder.PreviousPage()
        {
            throw new NotImplementedException();
        }
    }
}
