using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages.UpdateUser.Models
{
    public class UpdateUserActionContextModel
    {
        public string? Nickname { get; set; }
        public string? About { get; set; }
        public List<string>? Links { get; set; }
        public bool? IsRadyForOrders { get; set; }
        public List<ImageModel>? Images { get; set; }
    }
}
