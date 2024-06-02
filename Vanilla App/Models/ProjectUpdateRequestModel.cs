﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Common.Enums;

namespace Vanilla_App.Models
{
    public class ProjectUpdateRequestModel
    {
        public required Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DevelopmentStatusEnum? ProjectRequest { get; set; }
        public IEnumerable<string>? Links { get; set; }
    }
}