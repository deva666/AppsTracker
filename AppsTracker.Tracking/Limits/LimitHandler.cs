using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using AppsTracker.Common.Utils;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Communication;

namespace AppsTracker.Tracking.Helpers
{
    [Export(typeof(ILimitHandler))]
    internal sealed class LimitHandler : ILimitHandler
    {
        private readonly ISyncContext syncContext;
        private readonly IMediator mediator;
        private readonly IXmlSettingsService xmlSettingsService;

        [ImportingConstructor]
        public LimitHandler(ISyncContext syncContext,
                            IMediator mediator,
                            IXmlSettingsService xmlSettingsService)
        {
            this.syncContext = syncContext;
            this.mediator = mediator;
            this.xmlSettingsService = xmlSettingsService;
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
            if (xmlSettingsService.LimitsSettings.DontShowLimits.Any(l => l.AppLimitID == limit.AppLimitID))
                return;

            syncContext.Invoke(() =>
            {
                mediator.NotifyColleagues(MediatorMessages.APP_LIMIT_REACHED, limit);
            });
        }

        private void ShutdownApp(Aplication app)
        {
            try
            {
                var processes = Process.GetProcessesByName(app.WinName);
                foreach (var proc in processes)
                {
                    proc.Kill();
                }
            }
            catch
            {
            }
        }
    }
}
