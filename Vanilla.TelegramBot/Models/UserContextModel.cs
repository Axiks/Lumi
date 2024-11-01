using System.Resources;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Resources.Texts;
using Vanilla.TelegramBot.Services.Bot;

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

            timer = new(interval: 1000);
            timer.AutoReset = false;
            timer.Elapsed += (sender, e) => FinishUploadingPhotosEvent.Invoke();
            timer.Elapsed += (sender, e) => timer.Dispose();
            timer.Start();


     /*       DateTime now = DateTime.Now;
            DateTime fourOClock = DateTime.Today.AddSeconds(1);

            await Task.Delay((int)fourOClock.Subtract(DateTime.Now).TotalMilliseconds);
            FinishUploadingPhotosEvent.Invoke();*/
        }



        private ResourceManager _resourceManager;

        public List<int> SendMessages { get; set; } = new List<int>();

        public readonly UserModel User;
        public ResourceManager ResourceManager { get { return _resourceManager; } }
        public BotCreateProjectModel? CreateProjectContext { get; set; }
        public BotUpdateProjectModel? UpdateProjectContext { get; set; }
        public BotUpdateUserModel? UpdateUserContext { get; set; }

        public BotProjectCreator? BotProjectCreator { get; set; }
        public BotProjectUpdate? BotProjectUpdater { get; set; }
        public BotUserCreator? BotUserCreator { get; set; }





        public IFolder Folder { get; set; }

        public UserContextModel(UserModel user)
        {
            User = user;

            user.LanguageCode = "ua";
            _resourceManager = user.LanguageCode == "ua" || user.LanguageCode == "ru" ? new ResourceManager("Vanilla.TelegramBot.Resources.Texts.Ukrainian", typeof(Ukrainian).Assembly) : new ResourceManager("Vanilla.TelegramBot.Resources.Texts.English", typeof(English).Assembly);
        }
    }
}
