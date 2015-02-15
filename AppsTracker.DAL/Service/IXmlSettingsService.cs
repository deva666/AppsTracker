using AppsTracker.Models.XmlSettings;

namespace AppsTracker.DAL.Service
{
    public interface IXmlSettingsService : IBaseService
    {
        LogsViewSettings LogsViewSettings { get; }
        KeylogsViewSettings KeylogsViewSettings { get; }
        ScreenshotsViewSettings ScreenshotsViewSettings { get; }
        DaysViewSettings DaysViewSettings { get; }
        void Initialize();
        void ShutDown();
    }
}
