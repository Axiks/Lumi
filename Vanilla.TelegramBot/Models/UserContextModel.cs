using System.Resources;
using Vanilla.TelegramBot.Resources.Texts;
using Vanilla.TelegramBot.Services.Bot;

namespace Vanilla.TelegramBot.Models
{
    public class UserContextModel
    {
        private ResourceManager _resourceManager;


        public readonly UserModel User;
        public ResourceManager ResourceManager { get { return _resourceManager; } }
        public BotCreateProjectModel? CreateProjectContext {  get; set; }
        public BotUpdateProjectModel? UpdateProjectContext {  get; set; }
        public BotProjectCreator? BotProjectCreator { get; set; }
        public BotProjectUpdate? BotProjectUpdater { get; set; }

        public UserContextModel(UserModel user)
        {
            User = user;

            _resourceManager = user.LanguageCode == "ua" || user.LanguageCode == "ru" ? new ResourceManager("Vanilla.TelegramBot.Resources.Texts.Ukrainian", typeof(Ukrainian).Assembly) : new ResourceManager("Vanilla.TelegramBot.Resources.Texts.English", typeof(English).Assembly);
        }
    }
}
