using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.OAuth.Models
{
    public class UserUpdateRequestModel
    {
        public required Guid UserId { get; set; }
        public string Username { get; set; }
    }
}
