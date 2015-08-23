using System.ComponentModel;

namespace AppsTracker
{
    public enum TrackingStatus 
    {
        [Description("Tracking in progress ...")]
        Running,
        [Description("Tracking stopped")]
        Stopped
    }
}
