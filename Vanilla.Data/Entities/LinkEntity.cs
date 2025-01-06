namespace Vanilla.Data.Entities
{
    public class LinkEntity
    {
        public Guid Id { get; set; }
        public required string Url { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;
    }
}
