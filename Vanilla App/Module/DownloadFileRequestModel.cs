using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla_App.Module
{
    public class DownloadFileRequestModel
    {
        public required string DownloadURL { get; init; }
        public string? FileName { get; init; }
    }
}
