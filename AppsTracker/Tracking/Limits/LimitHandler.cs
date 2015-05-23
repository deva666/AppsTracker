using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;
using AppsTracker.Views;

namespace AppsTracker.Tracking.Helpers
{
    [Export(typeof(ILimitHandler))]
    internal sealed class LimitHandler : ILimitHandler
    {
        private readonly IWindowService windowService;
        private readonly IDataService dataService;
        private readonly ISyncContext syncContext;

        [ImportingConstructor]
        public LimitHandler(IWindowService windowService,
                            IDataService dataService,
                            ISyncContext syncContext)
        {
            this.windowService = windowService;
            this.dataService = dataService;
            this.syncContext = syncContext;
        }


        public void Handle(AppLimit limit)
        {
            Ensure.NotNull(limit, "limit");

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
            syncContext.Invoke(() =>
            {
                var toastShell = windowService.GetShell("Limit toast window");
                toastShell.ViewArgument = limit;
                toastShell.Show();
            });
        }

        private void ShutdownApp(Aplication app)
        {
            var processes = Process.GetProcessesByName(app.WinName);
            foreach (var proc in processes)
            {
                proc.Kill();
            }
        }
    }
}
