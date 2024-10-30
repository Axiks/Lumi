namespace Vanilla_App.Models
{
    public class UserModel
    {
        public required Guid Id { get; set; }
        public string? About { get; set; }
       public  List<string>? Links { get; set; }
        public bool IsRadyForOrders { get; set; } = false;

    }
}
