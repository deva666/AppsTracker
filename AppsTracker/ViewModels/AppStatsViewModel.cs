#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;


namespace AppsTracker.ViewModels
{
    internal sealed class AppStatsViewModel : ViewModelBase, ICommunicator
    {
        private readonly IStatsService statsService;


        public override string Title
        {
            get { return "APPS"; }
        }


        private AppDuration selectedApp;

        public AppDuration SelectedApp
        {
            get { return selectedApp; }
            set
            {
                SetPropertyValue(ref selectedApp, value);
                dailyAppList.Reload();
            }
        }


        public object SelectedItem { get; set; }


        private readonly AsyncProperty<IEnumerable<AppDuration>> appsList;

        public AsyncProperty<IEnumerable<AppDuration>> AppsList
        {
            get { return appsList; }
        }


        private readonly AsyncProperty<IEnumerable<DailyAppDuration>> dailyAppList;

        public AsyncProperty<IEnumerable<DailyAppDuration>> DailyAppList
        {
            get { return dailyAppList; }
        }


        private ICommand returnFromDetailedViewCommand;

        public ICommand ReturnFromDetailedViewCommand
        {
            get { return returnFromDetailedViewCommand ?? (returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView)); }
        }


        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        private void ReturnFromDetailedView()
        {
            SelectedApp = null;
        }


        public AppStatsViewModel()
        {
            statsService = ServiceFactory.Get<IStatsService>();

            appsList = new AsyncProperty<IEnumerable<AppDuration>>(GetApps, this);
            dailyAppList = new AsyncProperty<IEnumerable<DailyAppDuration>>(GetDailyApp, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadAll));
        }


        private void ReloadAll()
        {
            appsList.Reload();
            dailyAppList.Reload();
        }


        private IEnumerable<AppDuration> GetApps()
        {
            return statsService.GetAppsDuration(Globals.SelectedUserID, Globals.DateFrom, Globals.DateTo);
        }


        private IEnumerable<DailyAppDuration> GetDailyApp()
        {
            var app = selectedApp;
            if (app == null)
                return null;

            return statsService.GetAppDurationByDate(Globals.SelectedUserID, app.Name, Globals.DateFrom, Globals.DateTo);
        }
    }
}
