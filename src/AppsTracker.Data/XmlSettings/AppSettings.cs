
namespace AppsTracker.Data.XmlSettings
{
    public sealed class AppSettings : XmlSettingsBase
    {
        [SettingsNode(true)]
        public bool NotifyForNewVersion { get; set; }
    }
}
