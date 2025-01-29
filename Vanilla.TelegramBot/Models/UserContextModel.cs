using System.Collections.Generic;
using System.Resources;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Resources.Texts;
using Vanilla.TelegramBot.Services.Bot_Service;

namespace Vanilla.TelegramBot.Models
{
    public delegate void FinishUploadingPhotosHandler();

    public class UserContextModel
    {
        public readonly UpdateUserData UpdateUser;
        public UserModel? User;
        public IFolder? Folder { get; set; }
        public CoreMessageMenager MessageMenager { get; init; }

        readonly ResourceManager _resourceManager;
        public ResourceManager ResourceManager { get { return _resourceManager; } }

        public UserContextModel(UpdateUserData updateUser)
        {
            updateUser.LanguageCode = "ua"; // temp fix
            _resourceManager = updateUser.LanguageCode == "ua" || updateUser.LanguageCode == "ru" ? new ResourceManager("Vanilla.TelegramBot.Resources.Texts.Ukrainian", typeof(Ukrainian).Assembly) : new ResourceManager("Vanilla.TelegramBot.Resources.Texts.English", typeof(English).Assembly);

            MessageMenager = new CoreMessageMenager();
            UpdateUser = updateUser;
        }

        public UserContextModel(UpdateUserData updateUser, UserModel user)
        {
            user.LanguageCode = "ua"; // temp fix
            _resourceManager = user.LanguageCode == "ua" || user.LanguageCode == "ru" ? new ResourceManager("Vanilla.TelegramBot.Resources.Texts.Ukrainian", typeof(Ukrainian).Assembly) : new ResourceManager("Vanilla.TelegramBot.Resources.Texts.English", typeof(English).Assembly);

            MessageMenager = new CoreMessageMenager();
            User = user;
            UpdateUser = updateUser;
        }

        //public bool IsHasProfile => User is not null;
        public List<RoleEnum> Roles
        {
            get
            {
                List<RoleEnum> roles = new List<RoleEnum>();
                if (User is not null) roles.Add(RoleEnum.User);
                else roles.Add(RoleEnum.Anonim);

                if(UpdateUser.IsAdmin is true) roles.Add(RoleEnum.Admin);

                return roles;
            }
        }



        public event FinishUploadingPhotosHandler? FinishUploadingPhotosEvent;
        DateTime? LastLoadPhotoTime { get; set; }

        System.Timers.Timer timer = new(interval: 1000);
        public async void UpdateLoadPhotoTimer()
        {
            timer.Dispose();

            timer = new(interval: 2000); // fix
            timer.AutoReset = false;
            if (FinishUploadingPhotosEvent is not null) timer.Elapsed += (sender, e) => FinishUploadingPhotosEvent.Invoke();
            timer.Elapsed += (sender, e) => timer.Dispose();
            timer.Start();
        }


        //public List<int> SendMessages { get; set; } = new List<int>(); // delete in future
        //public BotUpdateUserModel? UpdateUserContext { get; set; } // delete in future

    }
}
