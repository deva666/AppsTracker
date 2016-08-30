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
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Settings;
using AppsTracker.Service;

namespace AppsTracker.Controllers
{
    [Export()]
    public sealed class ApplicationController
    {
        private readonly IAppearanceController appearanceController;
        private readonly ITrackingController trackingController;
        private readonly IUserSettingsService userSettingsService;
        private readonly IAppSettingsService appSettingsService;
        private readonly IRepository repository;
        private readonly IWindowService windowService;

        [ImportingConstructor]
        public ApplicationController(IAppearanceController appearanceController,
                                     ITrackingController trackingController,
                                     IAppSettingsService appSettingsService,
                                     IUserSettingsService userSettingsService,
                                     IRepository repository,
                                     IWindowService windowService)
        {
            this.appearanceController = appearanceController;
            this.trackingController = trackingController;
            this.userSettingsService = userSettingsService;
            this.appSettingsService = appSettingsService;
            this.repository = repository;
            this.windowService = windowService;
        }

        public void Initialize(bool autoStart)
        {
            userSettingsService.Initialize();
            PropertyChangedEventManager.AddHandler(appSettingsService, OnSettingsChanged, "Settings");

            repository.CheckUnfinishedEntries();
            repository.DbSizeCritical += OnDbSizeCritical;
            repository.GetDBSize();

            appearanceController.Initialize(appSettingsService.Settings);
            trackingController.Initialize(appSettingsService.Settings);

            if (autoStart == false)
            {
                windowService.OpenMainWindow();
                windowService.FirstRunWindowSetup();
            }
            
            EntryPoint.SingleInstanceManager.SecondInstanceActivating += (s, e) => windowService.OpenMainWindow();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            trackingController.SettingsChanging(appSettingsService.Settings);
            appearanceController.SettingsChanging(appSettingsService.Settings);
        }

        private async void OnDbSizeCritical(object sender, EventArgs e)
        {
            var settings = appSettingsService.Settings;
            settings.TakeScreenshots = false;
            await appSettingsService.SaveChangesAsync(settings);

            windowService.ShowMessageDialog("Database size has reached the maximum allowed value"
                + Environment.NewLine + "Please run the screenshot cleaner from the settings menu to continue capturing screenshots.", false);

            repository.DbSizeCritical -= OnDbSizeCritical;
        }

        public void Shutdown()
        {
            windowService.Shutdown();
            userSettingsService.Shutdown();
            trackingController.Shutdown();
        }
    }
}
