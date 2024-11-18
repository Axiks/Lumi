﻿using System.Resources;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Resources.Texts;

namespace Vanilla.TelegramBot.Models
{
    public delegate void FinishUploadingPhotosHandler();

    public class UserContextModel
    {
        public event FinishUploadingPhotosHandler? FinishUploadingPhotosEvent;
        DateTime? LastLoadPhotoTime { get; set; }


        System.Timers.Timer timer = new(interval: 1000);

        public async void UpdateLoadPhotoTimer()
        {
            timer.Dispose();

            timer = new(interval: 2000); // fix
            timer.AutoReset = false;
            if(FinishUploadingPhotosEvent is not null) timer.Elapsed += (sender, e) => FinishUploadingPhotosEvent.Invoke();
            timer.Elapsed += (sender, e) => timer.Dispose();
            timer.Start();
        }



        private ResourceManager _resourceManager;

        public List<int> SendMessages { get; set; } = new List<int>();

        public readonly UserModel User;
        public ResourceManager ResourceManager { get { return _resourceManager; } }

        public BotUpdateUserModel? UpdateUserContext { get; set; }





        public IFolder Folder { get; set; }

        public UserContextModel(UserModel user)
        {
            User = user;

            user.LanguageCode = "ua";
            _resourceManager = user.LanguageCode == "ua" || user.LanguageCode == "ru" ? new ResourceManager("Vanilla.TelegramBot.Resources.Texts.Ukrainian", typeof(Ukrainian).Assembly) : new ResourceManager("Vanilla.TelegramBot.Resources.Texts.English", typeof(English).Assembly);
        }
    }
}
