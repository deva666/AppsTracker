using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AppsTracker.Utils
{
    internal sealed class LocalSettings
    {
        private const string SETTINGS_FILE_NAME = "local settings.xml";

        private XmlDocument xml;

        public LocalSettings(string path)
        {
            var fullPath = Path.Combine(path, SETTINGS_FILE_NAME);
            if (File.Exists(fullPath))
            {
                xml.Load(fullPath);
            }
            else
            {
                xml = new XmlDocument();
            }
        }
    }
}
