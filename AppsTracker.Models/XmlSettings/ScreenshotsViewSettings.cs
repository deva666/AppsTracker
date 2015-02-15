using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Models.XmlSettings
{
    public sealed class ScreenshotsViewSettings : SettingsBase
    {
        [SettingsNode]
        public double SeparatorPosition { get; set; }
    }
}
