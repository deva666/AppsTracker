
namespace AppsTracker.Domain.Settings
{
    public sealed class AppSettings : XmlSettingsBase
    {
        [SettingsNode(false)]
        public bool DisableNotifyForNewVersion { get; set; }

        [SettingsNode(false)]
        public bool NotifyScreenshotTaken { get; set; }

        public AppSettings Clone()
        {
            return new AppSettings() { DisableNotifyForNewVersion = this.DisableNotifyForNewVersion, NotifyScreenshotTaken = this.NotifyScreenshotTaken };
        }
    }
}
