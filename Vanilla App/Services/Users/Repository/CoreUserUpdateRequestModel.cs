﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla_App.Services.Users.Repository
{
    public struct CoreUserUpdateRequestModel
    {
        public string? About { get; set; }
        public List<string>? Links { get; set; }
        public bool? IsRadyForOrders { get; set; }
    }
}
