namespace Vanilla.Common.Models
{
    public class TokenConfiguration
    {
        public required string PrivateKey { get; set; }
        public required int LifetimeSec { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }


    }
}
