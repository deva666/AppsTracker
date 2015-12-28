#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Data.Utils;
using AppsTracker.MVVM;
using AppsTracker.Tracking;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class DaySummaryViewModel : ViewModelBase
    {
        private readonly IDataService dataService;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;


        public override string Title
        {
            get { return "DAY SUMMARY"; }
        }


        public string DayOfWeek
        {
            get { return selectedDate.DayOfWeek.ToString(); }
        }


        private DateTime selectedDate = DateTime.Today;

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                PropertyChanging("SelectedDate");
                PropertyChanging("DayOfWeek");
                ReloadContent();
            }
        }


        private string selectedWindowsDuration;

        public string SelectedWindowsDuration
        {
            get { return selectedWindowsDuration; }
            set { SetPropertyValue(ref selectedWindowsDuration, value); }
        }


        private AppSummary selectedApp;

        public AppSummary SelectedApp
        {
            get { return selectedApp; }
            set
            {
                SetPropertyValue(ref selectedApp, value);
                SelectedWindowsDuration = string.Empty;
                if (value != null)
                    windowsList.Reload();
            }
        }


        private readonly AsyncProperty<IEnumerable<AppSummary>> appsList;

        public AsyncProperty<IEnumerable<AppSummary>> AppsList
        {
            get { return appsList; }
        }


        private readonly AsyncProperty<IEnumerable<LogSummary>> logsList;

        public AsyncProperty<IEnumerable<LogSummary>> LogsList
        {
            get { return logsList; }
        }


        private readonly AsyncProperty<IEnumerable<WindowSummary>> windowsList;

        public AsyncProperty<IEnumerable<WindowSummary>> WindowsList
        {
            get { return windowsList; }
        }


        private readonly AsyncProperty<IEnumerable<UsageByTime>> usageList;

        public AsyncProperty<IEnumerable<UsageByTime>> UsageList
        {
            get { return usageList; }
        }


        private readonly AsyncProperty<IEnumerable<CategoryDuration>> categoryList;

        public AsyncProperty<IEnumerable<CategoryDuration>> CategoryList
        {
            get { return categoryList; }
        }


        private ICommand selectedWindowsChangingCommand;

        public ICommand SelectedWindowsChangingCommand
        {
            get { return selectedWindowsChangingCommand ?? (selectedWindowsChangingCommand = new DelegateCommand(SelectedWindowsChanging)); }
        }


        private ICommand changeDateCommand;

        public ICommand ChangeDateCommand
        {
            get { return changeDateCommand ?? (changeDateCommand = new DelegateCommand(ChangeDate)); }
        }



        [ImportingConstructor]
        public DaySummaryViewModel(IDataService dataService,
                                   ITrackingService trackingService,
                                   IMediator mediator)
        {
            this.dataService = dataService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            logsList = new TaskRunner<IEnumerable<LogSummary>>(GetLogSummary, this);
            appsList = new TaskRunner<IEnumerable<AppSummary>>(GetAppsSummary, this);
            usageList = new TaskRunner<IEnumerable<UsageByTime>>(GetUsageSummary, this);
            windowsList = new TaskRunner<IEnumerable<WindowSummary>>(GetWindowsSummary, this);
            categoryList = new TaskRunner<IEnumerable<CategoryDuration>>(GetCategories, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadContent));
        }


        private void ReloadContent()
        {
            logsList.Reload();
            appsList.Reload();
            usageList.Reload();
            windowsList.Reload();
            categoryList.Reload();
        }


        private IEnumerable<LogSummary> GetLogSummary()
        {
            var dateTo = selectedDate.AddDays(1);

            var logsTask = dataService.GetFilteredAsync<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                               && l.DateCreated >= selectedDate
                               && l.DateCreated <= dateTo,
                               l => l.Window.Application);

            var usagesTask = dataService.GetFilteredAsync<Usage>(u => u.User.UserID == trackingService.SelectedUserID
                                     && u.UsageStart >= selectedDate
                                     && u.UsageEnd <= dateTo
                                     && u.UsageType != UsageTypes.Login);

            Task.WaitAll(logsTask, usagesTask);

            var logs = logsTask.Result;
            var usages = usagesTask.Result;

            var logModels = logs.Select(l => new LogSummary()
            {
                DateCreated = l.DateCreated.ToString("HH:mm:ss"),
                DateEnded = l.DateEnded.ToString("HH:mm:ss"),
                Duration = l.Duration,
                Name = l.Window.Application.Name,
                Title = l.Window.Title
            });

            var usageModels = usages.Select(u => new LogSummary()
            {
                DateCreated = u.UsageStart.ToString("HH:mm:ss"),
                DateEnded = u.UsageEnd.ToString("HH:mm:ss"),
                Duration = u.Duration.Ticks,
                Name = u.UsageType.ToExtendedString(),
                Title = "*********",
                IsRequested = true
            });

            return logModels.Union(usageModels).OrderBy(d => d.DateCreated).ToList();
        }


        private IEnumerable<AppSummary> GetAppsSummary()
        {
            var dateTo = selectedDate.AddDays(1);
            var logs = dataService.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                            && l.DateCreated >= selectedDate
                                            && l.DateCreated <= dateTo,
                                            l => l.Window.Application);

            Double totalDuration = (from l in logs
                                    select (Double?)l.Duration).Sum() ?? 0;

            var result = (from l in logs
                          group l by l.Window.Application.Name into grp
                          select grp).ToList()
                         .Select(g => new AppSummary
                         {
                             AppName = g.Key,
                             Date = selectedDate.ToShortDateString(),
                             Usage = (g.Sum(l => l.Duration) / totalDuration),
                             Duration = g.Sum(l => l.Duration)
                         })
                         .OrderByDescending(t => t.Duration)
                         .ToList();

            var first = result.FirstOrDefault();
            if (first != null)
                first.IsSelected = true;

            return result;
        }


        private IEnumerable<WindowSummary> GetWindowsSummary()
        {
            var model = selectedApp;
            if (model == null)
                return null;

            var nextDay = selectedDate.AddDays(1);

            var logs = dataService.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                                         && l.DateCreated >= selectedDate
                                                         && l.DateCreated <= nextDay
                                                         && l.Window.Application.Name == model.AppName,
                                                    l => l.Window);

            var totalDuration = logs.Sum(l => l.Duration);

            return logs.GroupBy(l => l.Window.Title)
                                  .Select(g => new WindowSummary
                                  {
                                      Title = g.Key,
                                      Usage = (g.Sum(l => l.Duration) / totalDuration),
                                      Duration = g.Sum(l => l.Duration)
                                  })
                                  .OrderByDescending(t => t.Duration)
                                  .ToList();
        }


        private IEnumerable<UsageByTime> GetUsageSummary()
        {
            var fromDay = selectedDate.Date;
            var nextDay = fromDay.AddDays(1d);
            var today = DateTime.Now.Date;

            var logins = dataService.GetFiltered<Usage>(u => u.User.UserID == trackingService.SelectedUserID
                                            && ((u.UsageStart >= fromDay && u.UsageStart <= nextDay)
                                                    || (u.IsCurrent && u.UsageStart < fromDay && today >= fromDay)
                                                    || (u.IsCurrent == false && u.UsageStart <= fromDay && u.UsageEnd >= fromDay))
                                            && u.UsageType == UsageTypes.Login);

            var usageIDs = logins.Select(u => u.UsageID);

            var allUsages = dataService.GetFiltered<Usage>(u => u.SelfUsageID.HasValue
                                                           && usageIDs.Contains(u.SelfUsageID.Value));

            var lockedUsages = allUsages.Where(u => u.UsageType == UsageTypes.Locked);
            var idleUsages = allUsages.Where(u => u.UsageType == UsageTypes.Idle);
            var stoppedUsages = allUsages.Where(u => u.UsageType == UsageTypes.Stopped);

            var usagesByTime = new List<UsageByTime>();

            foreach (var login in logins)
            {
                var series = new UsageByTime() { Time = login.GetDisplayedStart(fromDay).ToString("HH:mm:ss") };
                var observableCollection = new ObservableCollection<UsageSummary>();

                long idleTime = 0;
                long lockedTime = 0;
                long loginTime = 0;
                long stoppedTime = 0;

                var tempIdles = idleUsages.Where(u => u.SelfUsageID == login.UsageID);
                var tempLockeds = lockedUsages.Where(u => u.SelfUsageID == login.UsageID);
                var tempStopppeds = stoppedUsages.Where(u => u.SelfUsageID == login.UsageID);

                idleTime = tempIdles.Sum(l => l.GetDisplayedTicks(fromDay));
                if (idleTime > 0)
                    observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(idleTime).TotalHours, 2), UsageType = "Idle" });

                lockedTime = tempLockeds.Sum(l => l.GetDisplayedTicks(fromDay));
                if (lockedTime > 0)
                    observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Computer locked" });

                stoppedTime = tempStopppeds.Sum(l => l.GetDisplayedTicks(fromDay));
                if (stoppedTime > 0)
                    observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Stopped logging" });

                loginTime = login.GetDisplayedTicks(fromDay) - lockedTime - idleTime - stoppedTime;
                observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(loginTime).TotalHours, 2), UsageType = "Work" });

                series.UsageSummaryCollection = observableCollection;

                usagesByTime.Add(series);
            }

            if (logins.Count() > 1)
            {
                var seriesTotal = new UsageByTime() { Time = "TOTAL" };
                var observableTotal = new ObservableCollection<UsageSummary>();

                long idleTimeTotal = 0;
                long lockedTimeTotal = 0;
                long loginTimeTotal = 0;
                long stoppedTimeTotal = 0;

                idleTimeTotal = idleUsages.Sum(l => l.GetDisplayedTicks(fromDay));
                if (idleTimeTotal > 0)
                    observableTotal.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(idleTimeTotal).TotalHours, 2), UsageType = "Idle" });

                lockedTimeTotal = lockedUsages.Sum(l => l.GetDisplayedTicks(fromDay));
                if (lockedTimeTotal > 0)
                    observableTotal.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedTimeTotal).TotalHours, 2), UsageType = "Computer locked" });

                stoppedTimeTotal = stoppedUsages.Sum(l => l.GetDisplayedTicks(fromDay));
                if (stoppedTimeTotal > 0)
                    observableTotal.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(stoppedTimeTotal).TotalHours, 2), UsageType = "Stopped logging" });

                loginTimeTotal = logins.Sum(l => l.GetDisplayedTicks(fromDay)) - lockedTimeTotal - idleTimeTotal;
                observableTotal.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(loginTimeTotal).TotalHours, 2), UsageType = "Work" });

                seriesTotal.UsageSummaryCollection = observableTotal;

                usagesByTime.Add(seriesTotal);
            }

            return usagesByTime;
        }


        private IEnumerable<CategoryDuration> GetCategories()
        {
            var dateTo = selectedDate.AddDays(1);
            var categoryModels = new List<CategoryDuration>();

            var categories = dataService.GetFiltered<AppCategory>(c => c.Applications.Count > 0 &&
                       c.Applications.Where(a => a.UserID == trackingService.SelectedUserID).Any() &&
                       c.Applications.SelectMany(a => a.Windows).SelectMany(w => w.Logs).Where(l => l.DateCreated >= selectedDate).Any() &&
                       c.Applications.SelectMany(a => a.Windows).SelectMany(w => w.Logs).Where(l => l.DateCreated <= dateTo).Any(),
                      c => c.Applications.Select(a => a.Windows.Select(w => w.Logs)),
                      c => c.Applications);

            foreach (var cat in categories)
            {
                var totalDuration = cat.Applications
                    .SelectMany(a => a.Windows)
                    .SelectMany(w => w.Logs)
                    .Where(l => l.DateCreated >= selectedDate && l.DateCreated <= dateTo)
                    .Sum(l => l.Duration);

                categoryModels.Add(new CategoryDuration()
                {
                    Name = cat.Name,
                    TotalTime = Math.Round(new TimeSpan(totalDuration).TotalHours, 2)
                });
            }

            return categoryModels;
        }


        private void SelectedWindowsChanging()
        {
            var topWindows = windowsList.Result;
            if (topWindows == null)
                return;

            long ticks = 0;
            var selected = topWindows.Where(t => t.IsSelected);
            foreach (var window in selected)
                ticks += window.Duration;
            if (ticks == 0)
                return;

            var timeSpan = new TimeSpan(ticks);
            SelectedWindowsDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }


        private void ChangeDate(object parameter)
        {
            string stringParameter = parameter as string;
            if (stringParameter == null)
                return;
            switch (stringParameter)
            {
                case "+":
                    SelectedDate = SelectedDate.AddDays(1d);
                    break;
                case "-":
                    SelectedDate = SelectedDate.AddDays(-1d);
                    break;
                default:
                    SelectedDate = DateTime.Today;
                    break;
            }
        }
    }
}
