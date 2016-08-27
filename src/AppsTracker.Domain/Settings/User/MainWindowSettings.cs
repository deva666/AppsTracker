﻿
namespace AppsTracker.Domain.Settings
{
    public sealed class MainWindowSettings : XmlSettingsBase
    {
        [SettingsNode]
        public double Left { get; set; }

        [SettingsNode]
        public double Top { get; set; }

        [SettingsNode]
        public double Width { get; set; }

        [SettingsNode]
        public double Height { get; set; }
    }
}