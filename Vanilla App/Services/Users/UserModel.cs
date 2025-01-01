namespace Vanilla_App.Services.Users
{
    public class UserModel
    {
        public required Guid Id { get; init; }
        public required string Nickname { get; init; }
        public string? About { get; set; }
        public List<string>? Links { get; set; }
        public bool IsRadyForOrders { get; set; }
        public List<ProfileImage> ProfileImages { get; set; }

    }

    public class ProfileImage
    {
        public required Guid Id { get; init; }
        public required string FileName { get; init; }
        public required string FileHref { get; init; }

    }
}
