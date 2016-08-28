using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Common.Communication;
using AppsTracker.Common.Logging;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Settings;

namespace AppsTracker.Tracking.Limits
{
    [Export(typeof(ILimitHandler))]
    internal sealed class LimitHandler : ILimitHandler
    {
        private readonly IMediator mediator;
        private readonly IUserSettingsService userSettingsService;
        private readonly ILogger logger;
        private readonly IShutdownService shutdownService;

        [ImportingConstructor]
        public LimitHandler(IMediator mediator,
                            IUserSettingsService userSettingsService,
                            ILogger logger,
                            IShutdownService shutdownService)
        {
            this.mediator = mediator;
            this.userSettingsService = userSettingsService;
            this.logger = logger;
            this.shutdownService = shutdownService;
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
            if (userSettingsService.LimitsSettings.DontShowLimits.Any(l => l == limit.AppLimitID))
                return;

            mediator.NotifyColleagues(MediatorMessages.APP_LIMIT_REACHED, limit);
        }

        private void ShutdownApp(Aplication app)
        {
            shutdownService.Shutdown(app.WinName);
        }
    }
}
