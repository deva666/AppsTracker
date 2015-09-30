using System.Collections.Generic;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.XmlSettings
{
    public sealed class LimitsSettings
    {
        private IList<AppLimit> dontShowLimits;

        public IList<AppLimit> DontShowLimits
        {
            get { return dontShowLimits ?? (dontShowLimits = new List<AppLimit>()); }
            set { dontShowLimits = value; }
        }

    }
}
