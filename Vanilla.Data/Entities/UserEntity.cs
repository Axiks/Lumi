namespace Vanilla.Data.Entities
{
    public class UserEntity
    {
        public required Guid Id { get; init; }
        public string? About { get; set; }
        public List<string>? Links { get; set; }
        public bool IsRadyForOrders { get; set; } = false;
        public List<ImageEntity> ProfileImages { get; set; }
    }
}
