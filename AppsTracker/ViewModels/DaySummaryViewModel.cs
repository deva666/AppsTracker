#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class DaySummaryViewModel : ViewModelBase, ICommunicator
    {
        private readonly IDataService dataService;
        private readonly IStatsService statsService;


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
                selectedApp = value;
                SelectedWindowsDuration = string.Empty;
                if (value != null)
                    windowsList.Reload();
                PropertyChanging("TopAppsSingle");
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


        private readonly AsyncProperty<IEnumerable<CategoryModel>> categoryList;

        public AsyncProperty<IEnumerable<CategoryModel>> CategoryList
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


        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        public DaySummaryViewModel()
        {
            dataService = ServiceFactory.Get<IDataService>();
            statsService = ServiceFactory.Get<IStatsService>();

            logsList = new AsyncProperty<IEnumerable<LogSummary>>(GetLogSummary, this);
            appsList = new AsyncProperty<IEnumerable<AppSummary>>(GetAppsSummary, this);
            usageList = new AsyncProperty<IEnumerable<UsageByTime>>(GetUsageSummary, this);
            windowsList = new AsyncProperty<IEnumerable<WindowSummary>>(GetWindowsSummary, this);
            categoryList = new AsyncProperty<IEnumerable<CategoryModel>>(GetCategories, this);
            dayDuration = new AsyncProperty<string>(GetDayDuration, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadContent));
        }


        private void ReloadContent()
        {
            logsList.Reload();
            appsList.Reload();
            usageList.Reload();
            dayDuration.Reload();
            windowsList.Reload();
            categoryList.Reload();
        }


        private IEnumerable<LogSummary> GetLogSummary()
        {
            return statsService.GetLogSummary(Globals.SelectedUserID, selectedDate);
        }


        private IEnumerable<AppSummary> GetAppsSummary()
        {
            return statsService.GetAllAppSummaries(Globals.SelectedUserID, selectedDate);
        }


        private IEnumerable<WindowSummary> GetWindowsSummary()
        {
            var model = selectedApp;
            if (model == null)
                return null;

            return statsService.GetWindowsSummary(Globals.SelectedUserID, model.AppName, selectedDate);
        }


        private string GetDayDuration()
        {
            var tuple = statsService.GetDayInfo(Globals.SelectedUserID, selectedDate);

            return string.Format("Day start: {0}   -   Day end: {1} \t Total duration: {2}", tuple.Item1, tuple.Item2, tuple.Item3);
        }


        private IEnumerable<UsageByTime> GetUsageSummary()
        {
            return statsService.GetUsageSummary(Globals.SelectedUserID, selectedDate);
        }


        private IEnumerable<CategoryModel> GetCategories()
        {
            return statsService.GetCategories(Globals.SelectedUserID, selectedDate);
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

            TimeSpan timeSpan = new TimeSpan(ticks);
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
