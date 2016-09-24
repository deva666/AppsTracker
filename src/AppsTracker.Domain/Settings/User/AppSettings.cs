
namespace AppsTracker.Domain.Settings
{
    public sealed class AppSettings : XmlSettingsBase
    {
        [SettingsNode(false)]
        public bool DisableNotifyForNewVersion { get; set; }
    }
}
