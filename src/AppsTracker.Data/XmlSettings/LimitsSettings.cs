using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.XmlSettings
{
    public class LimitsSettings
    {
        private IList<AppLimit> dontShowLimits;

        public IList<AppLimit> DontShowLimits
        {
            get { return dontShowLimits ?? (dontShowLimits = new List<AppLimit>()); }
            set { dontShowLimits = value; }
        }

    }
}
