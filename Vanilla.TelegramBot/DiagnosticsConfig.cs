using System.Diagnostics.Metrics;
using System.Security.Authentication.ExtendedProtection;

namespace Vanilla.TelegramBot
{
    public static class DiagnosticsConfig
    {
        public const string ServiceName = "TelegramBot";
        public static Meter Meter = new(ServiceName);
        public static Counter<int> RequestCounter = Meter.CreateCounter<int>("core.request.count");
        public static Counter<int> SearchRequestCounter = Meter.CreateCounter<int>("search.request.count");
    }
}
