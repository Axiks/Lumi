namespace Vanilla_App.Models
{
    public class UserModel
    {
        public required Guid Id { get; set; }
        public string? Nickname { get; set; }
        public string? About { get; set; }
        List<string> Links { get; set; } = new List<string>();
        public bool IsRadyForOrders { get; set; } = false;

    }
}
