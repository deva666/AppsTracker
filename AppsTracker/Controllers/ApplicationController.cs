#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using AppsTracker.Logging.Helpers;
using AppsTracker.Service;
using AppsTracker.Widgets;
using Microsoft.Win32;

namespace AppsTracker.Controllers
{
    [Export(typeof(IApplicationController))]
    internal sealed class ApplicationController : IApplicationController
    {
        private readonly IAppearanceController appearanceController;
        private readonly ILoggingController loggingController;
        private readonly ISyncContext syncContext;
        private readonly IXmlSettingsService xmlSettingsService;
        private readonly ISqlSettingsService sqlSettingsService;
        private readonly ILoggingService loggingService;
        private readonly IWindowService windowService;

        [ImportingConstructor]
        public ApplicationController(IAppearanceController appearanceController, ILoggingController loggingController,
                                     ISyncContext syncContext, ISqlSettingsService sqlSettingsService,
                                     IXmlSettingsService xmlSettingsService, ILoggingService loggingService,
                                     IWindowService windowService)
        {
            this.appearanceController = appearanceController;
            this.loggingController = loggingController;
            this.syncContext = syncContext;
            this.xmlSettingsService = xmlSettingsService;
            this.sqlSettingsService = sqlSettingsService;
            this.loggingService = loggingService;
            this.windowService = windowService;
        }

        public void Initialize(bool autoStart)
        {
            syncContext.Context = System.Threading.SynchronizationContext.Current;
            xmlSettingsService.Initialize();
            PropertyChangedEventManager.AddHandler(sqlSettingsService, OnSettingsChanged, "Settings");

            appearanceController.Initialize(sqlSettingsService.Settings);
            loggingController.Initialize(sqlSettingsService.Settings);

            if (autoStart == false)
                windowService.CreateOrShowMainWindow();

            windowService.FirstRunWindowSetup();
            windowService.InitializeTrayIcon();

            loggingService.DbSizeCritical += OnDbSizeCritical;
            loggingService.GetDBSize();

            EntryPoint.SingleInstanceManager.SecondInstanceActivating += (s, e) => windowService.CreateOrShowMainWindow();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            loggingController.SettingsChanging(sqlSettingsService.Settings);
            appearanceController.SettingsChanging(sqlSettingsService.Settings);
        }

        private async void OnDbSizeCritical(object sender, EventArgs e)
        {
            var settings = sqlSettingsService.Settings;
            settings.TakeScreenshots = false;
            await sqlSettingsService.SaveChangesAsync(settings);

            windowService.ShowDialog("Database size has reached the maximum allowed value"
                + Environment.NewLine + "Please run the screenshot cleaner from the settings menu to continue capturing screenshots.", false);

            loggingService.DbSizeCritical -= OnDbSizeCritical;
        }

        public void ShutDown()
        {
            windowService.Shutdown();
            xmlSettingsService.ShutDown();
            loggingController.Dispose();
        }
    }
}
