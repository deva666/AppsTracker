
namespace AppsTracker.Domain.Settings
{
    public sealed class LogsViewSettings : XmlSettingsBase
    {
        [SettingsNode]
        public double HorizontalSeparatorPosition { get; set; }

        [SettingsNode]
        public double VerticalSeparatorPosition { get; set; }
    }
}
