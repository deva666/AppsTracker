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
using AppsTracker.Data.Service;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class Data_dayViewModel : ViewModelBase, ICommunicator
    {
        private DateTime selectedDate = DateTime.Today;

        private string singleAppDuration;
        private string singleWindowDuration;

        private TopAppsModel topAppsSingle;

        private readonly AsyncProperty<IEnumerable<TopAppsModel>> topAppsList;
        private readonly AsyncProperty<IEnumerable<DayViewModel>> dayViewModelList;
        private readonly AsyncProperty<IEnumerable<TopWindowsModel>> topWindowsList;
        private readonly AsyncProperty<IEnumerable<DailyUsageTypeSeries>> chartList;
        private readonly AsyncProperty<string> duration;
        private readonly AsyncProperty<IEnumerable<CategoryModel>> categoryList;

        private ICommand singleAppSelectionChangedCommand;
        private ICommand singleWindowSelectionChangedCommand;
        private ICommand addDaysCommand;

        private readonly IDataService dataService;
        private readonly IChartService chartService;

        public override string Title
        {
            get { return "DAY VIEW"; }
        }

        public string DayOfWeek
        {
            get { return selectedDate.DayOfWeek.ToString(); }
        }

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

        public string SingleAppDuration
        {
            get { return singleAppDuration; }
            set { SetPropertyValue(ref singleAppDuration, value); }
        }

        public string SingleWindowDuration
        {
            get { return singleWindowDuration; }
            set { SetPropertyValue(ref singleWindowDuration, value); }
        }

        public TopAppsModel TopAppsSingle
        {
            get { return topAppsSingle; }
            set
            {
                topAppsSingle = value;
                SingleWindowDuration = string.Empty;
                if (value != null)
                    topWindowsList.Reload();
                PropertyChanging("TopAppsSingle");
            }
        }

        public AsyncProperty<string> Duration
        {
            get { return duration; }
        }

        public AsyncProperty<IEnumerable<TopAppsModel>> TopAppsList
        {
            get { return topAppsList; }
        }

        public AsyncProperty<IEnumerable<DayViewModel>> DayViewModelList
        {
            get { return dayViewModelList; }
        }

        public AsyncProperty<IEnumerable<TopWindowsModel>> TopWindowsList
        {
            get { return topWindowsList; }
        }

        public AsyncProperty<IEnumerable<DailyUsageTypeSeries>> ChartList
        {
            get { return chartList; }
        }

        public AsyncProperty<IEnumerable<CategoryModel>> CategoryList
        {
            get { return categoryList; }
        }

        public ICommand SingleAppSelectionChangedCommand
        {
            get { return singleAppSelectionChangedCommand ?? (singleAppSelectionChangedCommand = new DelegateCommand(SingleAppSelectionChanged)); }
        }

        public ICommand SingleWindowSelectionChangedCommand
        {
            get { return singleWindowSelectionChangedCommand ?? (singleWindowSelectionChangedCommand = new DelegateCommand(SingleWindowSelectionChanged)); }
        }

        public ICommand AddDaysCommand
        {
            get { return addDaysCommand ?? (addDaysCommand = new DelegateCommand(AddDays)); }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        public Data_dayViewModel()
        {
            dataService = ServiceFactory.Get<IDataService>();
            chartService = ServiceFactory.Get<IChartService>();

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadContent));

            dayViewModelList = new AsyncProperty<IEnumerable<DayViewModel>>(GetDayViewInfo, this);
            topAppsList = new AsyncProperty<IEnumerable<TopAppsModel>>(GetTopAppsSingle, this);
            chartList = new AsyncProperty<IEnumerable<DailyUsageTypeSeries>>(GetChartContent, this);
            duration = new AsyncProperty<string>(GetDayInfo, this);
            topWindowsList = new AsyncProperty<IEnumerable<TopWindowsModel>>(GetTopWindowsSingle, this);
            categoryList = new AsyncProperty<IEnumerable<CategoryModel>>(GetCategories, this);
        }

        private void ReloadContent()
        {
            dayViewModelList.Reload();
            topAppsList.Reload();
            chartList.Reload();
            duration.Reload();
            topWindowsList.Reload();
            categoryList.Reload();
        }

        private IEnumerable<DayViewModel> GetDayViewInfo()
        {
            return chartService.GetDayView(Globals.SelectedUserID, selectedDate);
        }

        private IEnumerable<TopAppsModel> GetTopAppsSingle()
        {
            return chartService.GetDayTopApps(Globals.SelectedUserID, selectedDate);
        }

        private IEnumerable<TopWindowsModel> GetTopWindowsSingle()
        {
            var model = topAppsSingle;
            if (model == null)
                return null;

            return chartService.GetDayTopWindows(Globals.SelectedUserID, model.AppName, selectedDate);
        }

        private string GetDayInfo()
        {
            var tuple = chartService.GetDayInfo(Globals.SelectedUserID, selectedDate);

            return string.Format("Day start: {0}   -   Day end: {1} \t Total duration: {2}", tuple.Item1, tuple.Item2, tuple.Item3);
        }

        private IEnumerable<DailyUsageTypeSeries> GetChartContent()
        {
            return chartService.GetDailySeries(Globals.SelectedUserID, selectedDate);
        }

        private IEnumerable<CategoryModel> GetCategories()
        {
            return chartService.GetCategoriesForDate(Globals.SelectedUserID, selectedDate);
        }

        private void SingleAppSelectionChanged()
        {
            var topApps = topAppsList.Result;
            if (topApps == null)
                return;

            long ticks = 0;
            var selected = topApps.Where(t => t.IsSelected);
            foreach (var app in selected)
                ticks += app.Duration;
            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            SingleAppDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

        private void SingleWindowSelectionChanged()
        {
            var topWindows = topWindowsList.Result;
            if (topWindows == null)
                return;

            long ticks = 0;
            var selected = topWindows.Where(t => t.IsSelected);
            foreach (var window in selected)
                ticks += window.Duration;
            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            SingleWindowDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

        private void AddDays(object parameter)
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
