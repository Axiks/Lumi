﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
