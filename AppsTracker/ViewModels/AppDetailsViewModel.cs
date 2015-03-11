#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal class AppDetailsViewModel : ViewModelBase, ICommunicator
    {
        private readonly IDataService dataService;
        private readonly IChartService chartService;

        private readonly AsyncProperty<IEnumerable<Aplication>> aplicationList;
        private readonly AsyncProperty<IEnumerable<TopAppsModel>> topAppsList;
        private readonly AsyncProperty<IEnumerable<TopWindowsModel>> topWindowsList;
        private readonly AsyncProperty<IEnumerable<DailyWindowSeries>> chartList;


        TopAppsModel topAppsOverall;

        List<MenuItem> allUsersList;


        private bool isChartVisible;
        public bool IsChartVisible
        {
            get { return isChartVisible; }
            set { SetPropertyValue(ref isChartVisible, value); }
        }

        public override string Title
        {
            get { return "APPS"; }
        }

        private string selectedAppsDuration;
        public string SelectedAppsDuration
        {
            get { return selectedAppsDuration; }
            set { SetPropertyValue(ref selectedAppsDuration, value); }
        }

        private string selectedWindowsDuration;
        public string SelectedWindowsDuration
        {
            get { return selectedWindowsDuration; }
            set { SetPropertyValue(ref selectedWindowsDuration, value); }
        }

        public AsyncProperty<IEnumerable<Aplication>> AplicationList
        {
            get
            {
                return aplicationList;
            }
        }
        public AsyncProperty<IEnumerable<TopAppsModel>> TopAppsList
        {
            get
            {
                return topAppsList;
            }
        }
        public AsyncProperty<IEnumerable<TopWindowsModel>> TopWindowsList
        {
            get
            {
                return topWindowsList;
            }
        }
        public AsyncProperty<IEnumerable<DailyWindowSeries>> ChartList
        {
            get
            {
                return chartList;
            }
        }

        public TopAppsModel TopAppsOverall
        {
            get
            {
                return topAppsOverall;
            }
            set
            {
                SetPropertyValue(ref topAppsOverall, value);
                SelectedWindowsDuration = string.Empty;
            }
        }

        Aplication selectedApp;
        public Aplication SelectedApp
        {
            get { return selectedApp; }
            set
            {
                SetPropertyValue(ref selectedApp, value);
                IsChartVisible = false;
                if (value != null)
                {
                    topAppsList.Reload();
                }
            }
        }

        public List<MenuItem> AllUsersList
        {
            get
            {
                GetAllUsers();
                return allUsersList;
            }
        }

        ICommand addProcessToBlockedListCommand;
        public ICommand AddProcessToBlockedListCommand
        {
            get { return addProcessToBlockedListCommand ?? (addProcessToBlockedListCommand = new DelegateCommand(AddAplicationToBlockedList)); }
        }

        ICommand overallAppSelectionChangedCommand;
        public ICommand OverallAppSelectionChangedCommand
        {
            get { return overallAppSelectionChangedCommand ?? (overallAppSelectionChangedCommand = new DelegateCommand(OverallAppSelectionChanged)); }
        }

        ICommand overallWindowSelectionChangedCommand;
        public ICommand OverallWindowSelectionChangedCommand
        {
            get { return overallWindowSelectionChangedCommand ?? (overallWindowSelectionChangedCommand = new DelegateCommand(OverallWindowSelectionChanged)); }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        public AppDetailsViewModel()
        {
            dataService = ServiceFactory.Get<IDataService>();
            chartService = ServiceFactory.Get<IChartService>();

            aplicationList = new AsyncProperty<IEnumerable<Aplication>>(GetContent, this);
            topAppsList = new AsyncProperty<IEnumerable<TopAppsModel>>(GetTopApps, this);
            topWindowsList = new AsyncProperty<IEnumerable<TopWindowsModel>>(GetTopWindows, this);
            chartList = new AsyncProperty<IEnumerable<DailyWindowSeries>>(GetChartContent, this);


            Mediator.Register(MediatorMessages.ApplicationAdded, new Action<Aplication>(ApplicationAdded));
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(aplicationList.Reload));
        }

        private void LoadAppsOverall()
        {
            topAppsList.Reload();
            IsChartVisible = false;
        }

        private IEnumerable<Aplication> GetContent()
        {
            return dataService.GetFiltered<Aplication>(a => a.User.UserID == Globals.SelectedUserID
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= Globals.Date1).Any()
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated <= Globals.Date2).Any())
                                                           .ToList()
                                                           .Distinct();
        }

        private IEnumerable<TopAppsModel> GetTopApps()
        {
            var app = SelectedApp;
            if (app == null)
                return null;

            return chartService.GetLogTopApps(Globals.SelectedUserID, app.ApplicationID, app.Name, Globals.Date1, Globals.Date2);
        }

        private IEnumerable<TopWindowsModel> GetTopWindows()
        {
            var topApps = TopAppsOverall;
            if (TopAppsList.Result == null || topApps == null)
                return null;

            var days = TopAppsList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);
            return chartService.GetLogTopWindows(Globals.SelectedUserID, topApps.AppName, days);
        }

        private IEnumerable<DailyWindowSeries> GetChartContent()
        {
            var topApps = TopAppsOverall;

            if (TopAppsList.Result == null || topApps == null || TopWindowsList.Result == null)
                return null;

            var selectedWindows = TopWindowsList.Result.Where(w => w.IsSelected).Select(w => w.Title).ToList();
            var days = TopAppsList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);

            return chartService.GetDailyWindowSeries(Globals.SelectedUserID, topApps.AppName, selectedWindows, days);
        }


        private void ApplicationAdded(Aplication app)
        {
            aplicationList.Reload();

            chartList.Reset();
            topAppsList.Reset();
            topWindowsList.Reset();
        }

        private void AddAplicationToBlockedList(object parameter)
        {


        }

        private void OverallAppSelectionChanged()
        {
            IsChartVisible = false;

            if (TopAppsList.Result == null)
                return;

            long ticks = 0;
            var filteredApps = TopAppsList.Result.Where(t => t.IsSelected);

            if (filteredApps.Count() == 0)
            {
                topWindowsList.Reset();
                return;
            }

            foreach (var app in filteredApps)
                ticks += app.Duration;

            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            SelectedAppsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            topWindowsList.Reload();
        }

        private void OverallWindowSelectionChanged()
        {
            if (topWindowsList.Result == null)
            {
                ChartList.Reset();
                IsChartVisible = false;
                return;
            }

            if (topWindowsList.Result.Where(w => w.IsSelected).Count() == 0)
            {
                IsChartVisible = false;
                return;
            }

            IsChartVisible = true;
            long ticks = 0;

            foreach (var window in topWindowsList.Result.Where(w => w.IsSelected))
                ticks += window.Duration;

            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            SelectedWindowsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            chartList.Reload();
        }

        private void GetAllUsers()
        {
            if (allUsersList == null)
                allUsersList = new List<MenuItem>();

            MenuItem menuItem = new MenuItem();
            menuItem.Header = "All users";
            allUsersList.Add(menuItem);
            var users = dataService.GetFiltered<Uzer>(u => u.Name != null);

            foreach (var user in users)
            {
                menuItem = new MenuItem();
                menuItem.Header = user.Name;
                allUsersList.Add(menuItem);
            }
        }
    }
}
