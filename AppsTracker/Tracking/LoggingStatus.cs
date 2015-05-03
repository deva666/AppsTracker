using System.ComponentModel;

namespace AppsTracker
{
    public enum LoggingStatus : byte
    {
        [Description("Logging in progress ...")]
        Running,
        [Description("Logging stopped")]
        Stopped
    }
}
