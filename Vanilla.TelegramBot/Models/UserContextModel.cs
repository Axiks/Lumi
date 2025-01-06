using System.Resources;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Resources.Texts;

namespace Vanilla.TelegramBot.Models
{
    public delegate void FinishUploadingPhotosHandler();

    public class UserContextModel
    {
        public readonly UserModel User;
        public IFolder? Folder { get; set; }


        readonly ResourceManager _resourceManager;
        public ResourceManager ResourceManager { get { return _resourceManager; } }


        public List<int> SendMessages { get; set; } = new List<int>(); // delete in future
        public BotUpdateUserModel? UpdateUserContext { get; set; } // delete in future



        public UserContextModel(UserModel user)
        {
            user.LanguageCode = "ua"; // temp fix
            _resourceManager = user.LanguageCode == "ua" || user.LanguageCode == "ru" ? new ResourceManager("Vanilla.TelegramBot.Resources.Texts.Ukrainian", typeof(Ukrainian).Assembly) : new ResourceManager("Vanilla.TelegramBot.Resources.Texts.English", typeof(English).Assembly);

            User = user;
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

    }
}
