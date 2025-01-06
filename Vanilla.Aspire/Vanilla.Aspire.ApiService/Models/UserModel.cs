using Vanilla.Common.Message_Broker;

namespace Vanilla.Aspire.ApiService.Models
{
    public class UserModel : Vanilla_App.Services.Users.UserModel
    {
        //public List<string> ProfileImages { get; set; }
        public TgUserResponse TelegramData { get; set; }
    }
}
