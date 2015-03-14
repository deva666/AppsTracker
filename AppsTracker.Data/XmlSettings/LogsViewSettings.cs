using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Data.XmlSettings
{
    public sealed class LogsViewSettings : XmlSettingsBase
    {
        [SettingsNode]
        public double HorizontalSeparatorPosition { get; set; }
        
        [SettingsNode]
        public double VerticalSeparatorPosition { get; set; }
    }
}
