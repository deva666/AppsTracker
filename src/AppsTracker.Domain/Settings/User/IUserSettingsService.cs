
namespace AppsTracker.Domain.Settings
{
    public interface IUserSettingsService 
    {
        AppSettings AppSettings { get; set; }

        LogsViewSettings LogsViewSettings { get; }
                     
        ScreenshotsViewSettings ScreenshotsViewSettings { get; }
        
        DaysViewSettings DaysViewSettings { get; }
        
        MainWindowSettings MainWindowSettings { get; }

        LimitsSettings LimitsSettings { get; }
        
        void Initialize();
        
        void Shutdown();
    }
}
