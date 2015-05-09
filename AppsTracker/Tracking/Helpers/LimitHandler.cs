using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Helpers
{
    [Export(typeof(ILimitHandler))]
    internal sealed class LimitHandler : ILimitHandler
    {
        public void Handle(AppLimit limit)
        {
            switch (limit.LimitReachedAction)
            {
                case LimitReachedAction.Warn:
                    ShowWarning(limit);
                    break;
                case LimitReachedAction.Shutdown:
                    ShutdownApp(limit.Application);
                    break;
                case LimitReachedAction.WarnAndShutdown:
                    ShowWarning(limit);
                    ShutdownApp(limit.Application);
                    break;
                case LimitReachedAction.None:
                    break;
            }
        }

        private void ShowWarning(AppLimit limit)
        {
            //display toast window with app name, limit and duration
        }

        private void ShutdownApp(Aplication app)
        {
            //kill process
        }
    }
}
