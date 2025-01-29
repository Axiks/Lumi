using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Pages {
    public class UserActionContextModel
    {
        public required string Username { get; init; } //requried in the future
        public string Nickname { get; set; } //requried in the future
        public string? About { get; set; }
        public List<string>? Links { get; set; }
        public bool? IsRadyForOrders { get; set; }
        public List<ImageModel>? Images { get; set; }
    }
}
