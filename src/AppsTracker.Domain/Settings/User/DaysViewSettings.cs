
namespace AppsTracker.Data.XmlSettings
{
    public sealed class DaysViewSettings : XmlSettingsBase
    {
        [SettingsNode]
        public double SeparatorPosition { get; set; }
    }
}
