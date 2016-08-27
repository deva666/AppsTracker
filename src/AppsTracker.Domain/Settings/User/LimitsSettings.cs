using System.Collections.Generic;
using AppsTracker.Data.Models;

namespace AppsTracker.Domain.Settings
{
    public sealed class LimitsSettings
    {
        private IList<int> dontShowLimits;

        public IList<int> DontShowLimits
        {
            get { return dontShowLimits ?? (dontShowLimits = new List<int>()); }
            set { dontShowLimits = value; }
        }

    }
}
