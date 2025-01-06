namespace Vanilla.OAuth.Models
{
    public class BasicUserModel
    {
        public required Guid Id { get; set; }
        public required string Nickname { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Updated { get; set; }
    }
}
