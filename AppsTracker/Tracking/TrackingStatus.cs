using System.ComponentModel;

namespace AppsTracker
{
    public enum TrackingStatus : byte
    {
        [Description("Tracking in progress ...")]
        Running,
        [Description("Tracking stopped")]
        Stopped
    }
}
