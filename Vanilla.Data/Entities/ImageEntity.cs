using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.Data.Entities
{
    public class ImageEntity
    {
        public Guid Id { get; set; }
        public required string FileName { get; set; }
    }
}
