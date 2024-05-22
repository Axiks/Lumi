using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.OAuth.Models
{
    public class TokenConfiguration
    {
        public required string PrivateKey { get; set; }
        public required int LifetimeSec { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }

        
    }
}
