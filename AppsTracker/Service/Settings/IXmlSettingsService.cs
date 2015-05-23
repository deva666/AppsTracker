using AppsTracker.Data.XmlSettings;

namespace AppsTracker.Service
{
    public interface IXmlSettingsService : IBaseService
    {     
        LogsViewSettings LogsViewSettings { get; }
             
        KeylogsViewSettings KeylogsViewSettings { get; }
        
        ScreenshotsViewSettings ScreenshotsViewSettings { get; }
        
        DaysViewSettings DaysViewSettings { get; }
        
        MainWindowSettings MainWindowSettings { get; }

        LimitsSettings LimitsSettings { get; }
        
        void Initialize();
        
        void ShutDown();
    }
}
