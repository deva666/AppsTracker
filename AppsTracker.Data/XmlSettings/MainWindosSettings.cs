using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Data.XmlSettings
{
    public class MainWindowSettings : SettingsBase
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
