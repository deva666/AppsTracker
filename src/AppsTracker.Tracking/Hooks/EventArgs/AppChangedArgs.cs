using AppsTracker.Data.Utils;

namespace AppsTracker.Tracking.Hooks
{
    public struct AppChangedArgs
    {
        public LogInfo LogInfo { get; private set; }

        public AppChangedArgs(LogInfo logInfo)
        {
            LogInfo = logInfo;
        }
    }
}
