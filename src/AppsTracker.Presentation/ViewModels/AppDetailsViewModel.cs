#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Data.Repository;
using AppsTracker.Common.Communication;
using AppsTracker.Tracking;
using System.Threading.Tasks;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class AppDetailsViewModel : ViewModelBase
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;

        public override string Title
        {
            get { return "APPS"; }
        }


        private bool isChartVisible;

        public bool IsChartVisible
        {
            get { return isChartVisible; }
            set { SetPropertyValue(ref isChartVisible, value); }
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


        private readonly AsyncProperty<IEnumerable<Aplication>> appList;

        public AsyncProperty<IEnumerable<Aplication>> AppList
        {
            get { return appList; }
        }


        private readonly AsyncProperty<IEnumerable<AppSummary>> appSummaryList;

        public AsyncProperty<IEnumerable<AppSummary>> AppSummaryList
        {
            get { return appSummaryList; }
        }


        private readonly AsyncProperty<IEnumerable<WindowSummary>> windowSummaryList;

        public AsyncProperty<IEnumerable<WindowSummary>> WindowSummaryList
        {
            get { return windowSummaryList; }
        }


        private readonly AsyncProperty<IEnumerable<WindowDurationOverview>> windowDurationList;

        public AsyncProperty<IEnumerable<WindowDurationOverview>> WindowDurationList
        {
            get { return windowDurationList; }
        }


        private AppSummary selectedAppSummary;

        public AppSummary SelectedAppSummary
        {
            get { return selectedAppSummary; }
            set
            {
                SetPropertyValue(ref selectedAppSummary, value);
                SelectedWindowsDuration = string.Empty;
            }
        }


        private Aplication selectedApp;

        public Aplication SelectedApp
        {
            get { return selectedApp; }
            set
            {
                SetPropertyValue(ref selectedApp, value);
                IsChartVisible = false;
                if (value != null)
                {
                    appSummaryList.Reload();
                }
            }
        }


        private ICommand selectedAppsChangingCommand;

        public ICommand SelectedAppsChangingCommand
        {
            get { return selectedAppsChangingCommand ?? (selectedAppsChangingCommand = new DelegateCommand(SelectedAppChanged)); }
        }


        private ICommand selectedWindowsChangingCommand;

        public ICommand SelectedWindowsChangingCommand
        {
            get { return selectedWindowsChangingCommand ?? (selectedWindowsChangingCommand = new DelegateCommand(SelectedWindowChanged)); }
        }


        [ImportingConstructor]
        public AppDetailsViewModel(IRepository repository,
                                   ITrackingService trackingService,
                                   IMediator mediator)
        {
            this.repository = repository;
            this.trackingService = trackingService;
            this.mediator = mediator;

            appList = new TaskObserver<IEnumerable<Aplication>>(GetApps, this);
            appSummaryList = new TaskRunner<IEnumerable<AppSummary>>(GetAppSummary, this);
            windowSummaryList = new TaskRunner<IEnumerable<WindowSummary>>(GetWindowSummary, this);
            windowDurationList = new TaskRunner<IEnumerable<WindowDurationOverview>>(GetWindowDuration, this);

            this.mediator.Register(MediatorMessages.APPLICATION_ADDED, new Action<Aplication>(ApplicationAdded));
            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        private async Task<IEnumerable<Aplication>> GetApps()
        {
            return (await repository.GetFilteredAsync<Aplication>(a => a.User.UserID == trackingService.SelectedUserID
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= trackingService.DateFrom).Any()
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated <= trackingService.DateTo).Any()))
                                                           .Distinct();
        }

        private IEnumerable<AppSummary> GetAppSummary()
        {
            var app = SelectedApp;
            if (app == null)
                return null;

            var logs = repository.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                                && l.DateCreated >= trackingService.DateFrom
                                                && l.DateCreated <= trackingService.DateTo,
                                                l => l.Window.Application);

            var totalDuration = (from l in logs
                                 group l by new
                                 {
                                     year = l.DateCreated.Year,
                                     month = l.DateCreated.Month,
                                     day = l.DateCreated.Day
                                 } into grp
                                 select grp)
                                .Select(g => new
                                {
                                    Date = new DateTime(g.Key.year, g.Key.month, g.Key.day),
                                    Duration = (Double)g.Sum(l => l.Duration)
                                });


            var result = (from l in logs
                          where l.Window.Application.ApplicationID == app.ApplicationID
                          group l by new
                          {
                              year = l.DateCreated.Year,
                              month = l.DateCreated.Month,
                              day = l.DateCreated.Day,
                              name = l.Window.Application.Name
                          } into grp
                          select grp)                         
                          .Select(g => new AppSummary
                          {
                              AppName = g.Key.name,
                              Date = new DateTime(g.Key.year, g.Key.month, g.Key.day)
                                   .ToShortDateString()
                                   + " " + new DateTime(g.Key.year, g.Key.month, g.Key.day)
                                   .DayOfWeek.ToString(),
                              DateTime = new DateTime(g.Key.year, g.Key.month, g.Key.day),
                              Usage = g.Sum(l => l.Duration) / totalDuration
                                   .First(t => t.Date == new DateTime(g.Key.year, g.Key.month, g.Key.day)).Duration,
                              Duration = g.Sum(l => l.Duration)
                          })
                          .OrderByDescending(t => t.DateTime)
                          .ToList();

            var requestedApp = result.Where(a => a.AppName == app.Name).FirstOrDefault();

            if (requestedApp != null)
                requestedApp.IsSelected = true;

            return result;
        }

        private IEnumerable<WindowSummary> GetWindowSummary()
        {
            var selectedApp = SelectedAppSummary;
            if (AppSummaryList.Result == null || selectedApp == null)
                return null;

            var selectedDates = AppSummaryList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);

            var logs = repository.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                               && l.Window.Application.Name == selectedApp.AppName,
                                               l => l.Window);

            var logsInSelectedDateRange = logs.Where(l => selectedDates.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)));

            double totalDuration = logsInSelectedDateRange.Sum(l => l.Duration);

            var result = (from l in logsInSelectedDateRange
                          group l by l.Window.Title into grp
                          select grp)
                          .Select(g => new WindowSummary
                          {
                              Title = g.Key,
                              Usage = (g.Sum(l => l.Duration) / totalDuration),
                              Duration = g.Sum(l => l.Duration)
                          })
                          .OrderByDescending(t => t.Duration)
                          .ToList();

            return result;
        }


        private IEnumerable<WindowDurationOverview> GetWindowDuration()
        {
            var selectedApp = SelectedAppSummary;

            if (AppSummaryList.Result == null || selectedApp == null || WindowSummaryList.Result == null)
                return null;

            var selectedWindows = WindowSummaryList.Result.Where(w => w.IsSelected).Select(w => w.Title).ToList();
            if (selectedWindows.Count() == 0)
                return null;

            var selectedDates = AppSummaryList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);
            
            var logs = repository.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                                && l.Window.Application.Name == selectedApp.AppName,
                                                l => l.Window);


            var filteredLogs = logs.Where(l => selectedDates.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)) && selectedWindows.Contains(l.Window.Title));

            var result = new List<WindowDurationOverview>();

            var logsGroupedByDay = from l in filteredLogs
                                   group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into grp
                                   select grp;

            foreach (var grp in logsGroupedByDay)
            {
                var logsGroupedByWindowTitle = grp.GroupBy(g => g.Window.Title);
                var date = new DateTime(grp.Key.year, grp.Key.month, grp.Key.day);
                var series = new WindowDurationOverview()
                {
                    Date = date.ToShortDateString() + " " + date.DayOfWeek.ToString()
                };
                var modelList = new List<WindowDuration>();
                foreach (var grp2 in logsGroupedByWindowTitle)
                {
                    WindowDuration model = new WindowDuration() { Title = grp2.Key, Duration = Math.Round(new TimeSpan(grp2.Sum(l => l.Duration)).TotalMinutes, 2) };
                    modelList.Add(model);
                }
                series.DurationCollection = modelList;
                result.Add(series);
            }

            return result;
        }


        private void ApplicationAdded(Aplication app)
        {
            ReloadAll();
        }

        private void ReloadAll()
        {
            SelectedApp = null;
            SelectedAppSummary = null;

            appList.Reload();

            windowDurationList.Reset();
            appSummaryList.Reset();
            windowSummaryList.Reset();
        }

        private void SelectedAppChanged()
        {
            IsChartVisible = false;

            if (AppSummaryList.Result == null)
                return;

            long ticks = 0;
            var selectedApps = AppSummaryList.Result.Where(a => a.IsSelected);

            if (selectedApps.Count() == 0)
            {
                windowSummaryList.Reset();
                return;
            }

            foreach (var app in selectedApps)
                ticks += app.Duration;

            if (ticks == 0)
                return;

            var timeSpan = new TimeSpan(ticks);
            SelectedAppsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            windowSummaryList.Reload();
        }


        private void SelectedWindowChanged()
        {
            if (windowSummaryList.Result == null)
            {
                WindowDurationList.Reset();
                IsChartVisible = false;
                return;
            }

            if (windowSummaryList.Result.Where(w => w.IsSelected).Count() == 0)
            {
                IsChartVisible = false;
                return;
            }

            IsChartVisible = true;
            long ticks = 0;

            foreach (var window in windowSummaryList.Result.Where(w => w.IsSelected))
                ticks += window.Duration;

            if (ticks == 0)
                return;

            var timeSpan = new TimeSpan(ticks);
            SelectedWindowsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            windowDurationList.Reload();
        }
    }
}
