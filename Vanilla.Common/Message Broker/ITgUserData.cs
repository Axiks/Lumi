using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla.Common.Message_Broker
{
    public interface ITgUserData
    {
        string Username { get; }
        List<string> Images { get; }
    }
}
