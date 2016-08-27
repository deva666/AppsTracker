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
using AppsTracker.Service;

namespace AppsTracker.Controllers
{
    [Export()]
    internal sealed class ApplicationController
    {
        private readonly IAppearanceController appearanceController;
        private readonly ITrackingController trackingController;
        private readonly IXmlSettingsService xmlSettingsService;
        private readonly ISqlSettingsService sqlSettingsService;
        private readonly IRepository dataService;
        private readonly IWindowService windowService;

        [ImportingConstructor]
        public ApplicationController(IAppearanceController appearanceController,
                                     ITrackingController trackingController,
                                     ISqlSettingsService sqlSettingsService,
                                     IXmlSettingsService xmlSettingsService,
                                     IRepository dataService,
                                     IWindowService windowService)
        {
            this.appearanceController = appearanceController;
            this.trackingController = trackingController;
            this.xmlSettingsService = xmlSettingsService;
            this.sqlSettingsService = sqlSettingsService;
            this.dataService = dataService;
            this.windowService = windowService;
        }

        public void Initialize(bool autoStart)
        {
            xmlSettingsService.Initialize();
            PropertyChangedEventManager.AddHandler(sqlSettingsService, OnSettingsChanged, "Settings");

            appearanceController.Initialize(sqlSettingsService.Settings);
            trackingController.Initialize(sqlSettingsService.Settings);

            dataService.CheckUnfinishedEntries();
            dataService.DbSizeCritical += OnDbSizeCritical;
            dataService.GetDBSize();

            if (autoStart == false)
            {
                windowService.OpenMainWindow();
                windowService.FirstRunWindowSetup();
            }
            
            EntryPoint.SingleInstanceManager.SecondInstanceActivating += (s, e) => windowService.OpenMainWindow();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            trackingController.SettingsChanging(sqlSettingsService.Settings);
            appearanceController.SettingsChanging(sqlSettingsService.Settings);
        }

        private async void OnDbSizeCritical(object sender, EventArgs e)
        {
            var settings = sqlSettingsService.Settings;
            settings.TakeScreenshots = false;
            await sqlSettingsService.SaveChangesAsync(settings);

            windowService.ShowMessageDialog("Database size has reached the maximum allowed value"
                + Environment.NewLine + "Please run the screenshot cleaner from the settings menu to continue capturing screenshots.", false);

            dataService.DbSizeCritical -= OnDbSizeCritical;
        }

        public void Shutdown()
        {
            windowService.Shutdown();
            xmlSettingsService.Shutdown();
            trackingController.Shutdown();
        }
    }
}
