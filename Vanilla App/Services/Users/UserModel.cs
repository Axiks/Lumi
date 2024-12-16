namespace Vanilla_App.Services.Users
{
    public class UserModel
    {
        public required Guid Id { get; init; }
        public required string Nickname { get; init; }
        public string? About { get; set; }
        public List<string>? Links { get; set; }
        public bool IsRadyForOrders { get; set; }

    }
}
