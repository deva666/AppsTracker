
namespace AppsTracker.Domain.Settings
{
    public sealed class DaysViewSettings : XmlSettingsBase
    {
        [SettingsNode]
        public double SeparatorPosition { get; set; }
    }
}
