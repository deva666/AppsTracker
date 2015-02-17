using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Models.XmlSettings
{
    public sealed class LogsViewSettings : SettingsBase
    {
        [SettingsNode]
        public double HorizontalSeparatorPosition { get; set; }
        
        [SettingsNode]
        public double VerticalSeparatorPosition { get; set; }
    }
}
