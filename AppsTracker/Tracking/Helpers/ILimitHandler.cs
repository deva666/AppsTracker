using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Helpers
{
    interface ILimitHandler
    {
        void Handle(AppWarning warning);
    }
}
