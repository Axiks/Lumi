namespace Vanilla.OAuth.Models
{
    public class UserUpdateRequestModel
    {
        public required Guid UserId { get; set; }
        public string Username { get; set; }
    }
}
