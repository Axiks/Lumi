using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla_App.Services.Bonus
{
    public class ServerHealthModel
    {
        public string Status {  get; set; }
        public string Uptime {  get; set; }
        public string Version {  get; set; }
    }
}
