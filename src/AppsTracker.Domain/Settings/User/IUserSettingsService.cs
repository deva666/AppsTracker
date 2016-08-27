using AppsTracker.Data.XmlSettings;

namespace AppsTracker.Data.Repository
{
    public interface IUserSettingsService : IBaseService
    {
        AppSettings AppSettings { get; }

        LogsViewSettings LogsViewSettings { get; }
                     
        ScreenshotsViewSettings ScreenshotsViewSettings { get; }
        
        DaysViewSettings DaysViewSettings { get; }
        
        MainWindowSettings MainWindowSettings { get; }

        LimitsSettings LimitsSettings { get; }
        
        void Initialize();
        
        void Shutdown();
    }
}
