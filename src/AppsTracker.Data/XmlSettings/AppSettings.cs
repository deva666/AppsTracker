
namespace AppsTracker.Data.XmlSettings
{
    public sealed class AppSettings : XmlSettingsBase
    {
        [SettingsNode(false)]
        public bool DisableNotifyForNewVersion { get; set; }
    }
}
