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
using AppsTracker.Data.Service;
using AppsTracker.Common.Communication;

namespace AppsTracker.ViewModels
{
    [Export] 
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class DaySummaryViewModel : ViewModelBase
    {
        private readonly IDataService dataService;
        private readonly IStatsService statsService;
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


        private readonly AsyncProperty<string> dayDuration;

        public AsyncProperty<string> DayDuration
        {
            get { return dayDuration; }
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
                                   IStatsService statsService,
                                   ITrackingService trackingService,
                                   IMediator mediator)
        {
            this.dataService = dataService;
            this.statsService = statsService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            logsList = new AsyncProperty<IEnumerable<LogSummary>>(GetLogSummary, this);
            appsList = new AsyncProperty<IEnumerable<AppSummary>>(GetAppsSummary, this);
            usageList = new AsyncProperty<IEnumerable<UsageByTime>>(GetUsageSummary, this);
            windowsList = new AsyncProperty<IEnumerable<WindowSummary>>(GetWindowsSummary, this);
            categoryList = new AsyncProperty<IEnumerable<CategoryDuration>>(GetCategories, this);
            //dayDuration = new AsyncProperty<string>(GetDayDuration, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadContent));
        }


        private void ReloadContent()
        {
            logsList.Reload();
            appsList.Reload();
            usageList.Reload();
            //dayDuration.Reload();
            windowsList.Reload();
            categoryList.Reload();
        }


        private IEnumerable<LogSummary> GetLogSummary()
        {
            return statsService.GetLogSummary(trackingService.SelectedUserID, selectedDate);
        }


        private IEnumerable<AppSummary> GetAppsSummary()
        {
            return statsService.GetAllAppSummaries(trackingService.SelectedUserID, selectedDate);
        }


        private IEnumerable<WindowSummary> GetWindowsSummary()
        {
            var model = selectedApp;
            if (model == null)
                return null;

            return statsService.GetWindowsSummary(trackingService.SelectedUserID, model.AppName, selectedDate);
        }


        private string GetDayDuration()
        {
            var tuple = statsService.GetDayInfo(trackingService.SelectedUserID, selectedDate);

            return string.Format("Day start: {0}   -   Day end: {1} \t Total duration: {2}", tuple.Item1, tuple.Item2, tuple.Item3);
        }


        private IEnumerable<UsageByTime> GetUsageSummary()
        {
            return statsService.GetUsageSummary(trackingService.SelectedUserID, selectedDate);
        }


        private IEnumerable<CategoryDuration> GetCategories()
        {
            return statsService.GetCategories(trackingService.SelectedUserID, selectedDate);
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
