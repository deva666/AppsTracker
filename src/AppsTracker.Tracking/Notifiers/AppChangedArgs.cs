using AppsTracker.Data.Utils;

namespace AppsTracker.Tracking
{
    public struct AppChangedArgs
    {
        public LogInfo LogInfo { get; private set; }

        public AppChangedArgs(LogInfo logInfo) : this()
        {
            LogInfo = logInfo;
        }
    }
}
