
namespace AppsTracker.Domain.Settings
{
    public sealed class ScreenshotsViewSettings : XmlSettingsBase
    {
        [SettingsNode]
        public double SeparatorPosition { get; set; }
    }
}
