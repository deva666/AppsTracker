#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using AppsTracker.DAL.Service;
using AppsTracker.Models.ChartModels;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class Data_dayViewModel : ViewModelBase, ICommunicator
    {
        #region Fields

        private DateTime _selectedDate = DateTime.Today;

        private string _singleAppDuration;
        private string _singleWindowDuration;

        private TopAppsModel _topAppsSingle;

        private AsyncProperty<IEnumerable<TopAppsModel>> _topAppsList;
        private AsyncProperty<IEnumerable<DayViewModel>> _dayViewModelList;
        private AsyncProperty<IEnumerable<TopWindowsModel>> _topWindowsList;
        private AsyncProperty<IEnumerable<DailyUsageTypeSeries>> _chartList;
        private AsyncProperty<string> _duration;

        private ICommand _singleAppSelectionChangedCommand;
        private ICommand _singleWindowSelectionChangedCommand;
        private ICommand _addDaysCommand;

        private IAppsService _appsService;
        private IChartService _chartService;

        #endregion

        #region Properties

        public override string Title
        {
            get { return "DAY VIEW"; }
        }

        public string DayOfWeek
        {
            get { return _selectedDate.DayOfWeek.ToString(); }
        }

        public DateTime SelectedDate
        {
            get
            {
                return _selectedDate;
            }
            set
            {
                _selectedDate = value;
                PropertyChanging("SelectedDate");
                PropertyChanging("DayOfWeek");
                ReloadContent();
            }
        }

        public string SingleAppDuration
        {
            get
            {
                return _singleAppDuration;
            }
            set
            {
                _singleAppDuration = value;
                PropertyChanging("SingleAppDuration");
            }
        }
        public string SingleWindowDuration
        {
            get
            {
                return _singleWindowDuration;
            }
            set
            {
                _singleWindowDuration = value;
                PropertyChanging("SingleWindowDuration");
            }
        }
        public AsyncProperty<string> Duration
        {
            get
            {
                return _duration;
            }
        }
        public TopAppsModel TopAppsSingle
        {
            get
            {
                return _topAppsSingle;
            }
            set
            {
                _topAppsSingle = value;
                SingleWindowDuration = string.Empty;
                if (value != null)
                    _topWindowsList.Reload();
                PropertyChanging("TopAppsSingle");
            }
        }

        public AsyncProperty<IEnumerable<TopAppsModel>> TopAppsList
        {
            get
            {
                return _topAppsList;
            }
        }
        public AsyncProperty<IEnumerable<DayViewModel>> DayViewModelList
        {
            get
            {
                return _dayViewModelList;
            }
        }
        public AsyncProperty<IEnumerable<TopWindowsModel>> TopWindowsList
        {
            get
            {
                return _topWindowsList;
            }
        }
        public AsyncProperty<IEnumerable<DailyUsageTypeSeries>> ChartList
        {
            get
            {
                return _chartList;
            }
        }

        public ICommand SingleAppSelectionChangedCommand
        {
            get
            {
                return _singleAppSelectionChangedCommand == null ? _singleAppSelectionChangedCommand = new DelegateCommand(SingleAppSelectionChanged) : _singleAppSelectionChangedCommand;
            }
        }
        public ICommand SingleWindowSelectionChangedCommand
        {
            get
            {
                return _singleWindowSelectionChangedCommand == null ? _singleWindowSelectionChangedCommand = new DelegateCommand(SingleWindowSelectionChanged) : _singleWindowSelectionChangedCommand;
            }
        }
        public ICommand AddDaysCommand
        {
            get { return _addDaysCommand == null ? _addDaysCommand = new DelegateCommand(AddDays) : _addDaysCommand; }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }
        #endregion

        public Data_dayViewModel()
        {
            _appsService = ServiceFactory.Get<IAppsService>();
            _chartService = ServiceFactory.Get<IChartService>();

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadContent));
 
            _dayViewModelList = new AsyncProperty<IEnumerable<DayViewModel>>(GetDayViewInfo, this);
            _topAppsList = new AsyncProperty<IEnumerable<TopAppsModel>>(GetTopAppsSingle, this);
            _chartList = new AsyncProperty<IEnumerable<DailyUsageTypeSeries>>(GetChartContent, this);
            _duration = new AsyncProperty<string>(GetDayInfo, this);
            _topWindowsList = new AsyncProperty<IEnumerable<TopWindowsModel>>(GetTopWindowsSingle, this);
        }

        private void ReloadContent()
        {
            _dayViewModelList.Reload();
            _topAppsList.Reload();
            _chartList.Reload();
            _duration.Reload();
            _topWindowsList.Reload();
        }

        private IEnumerable<DayViewModel> GetDayViewInfo()
        {
            return _chartService.GetDayView(Globals.SelectedUserID, _selectedDate);
        }

        private IEnumerable<TopAppsModel> GetTopAppsSingle()
        {
            return _chartService.GetDayTopApps(Globals.SelectedUserID, _selectedDate);
        }

        private IEnumerable<TopWindowsModel> GetTopWindowsSingle()
        {
            var model = _topAppsSingle;
            if (model == null)
                return null;

            return _chartService.GetDayTopWindows(Globals.SelectedUserID, model.AppName, _selectedDate);
        }

        private string GetDayInfo()
        {
            var tuple = _chartService.GetDayInfo(Globals.SelectedUserID, _selectedDate);

            return string.Format("Day start: {0}   -   Day end: {1} \t Total duration: {2}", tuple.Item1, tuple.Item2, tuple.Item3);
        }

        private IEnumerable<DailyUsageTypeSeries> GetChartContent()
        {
            return _chartService.GetDailySeries(Globals.SelectedUserID, _selectedDate);
        }

        #region Commmand Methods

        private void SingleAppSelectionChanged()
        {
            var topApps = _topAppsList.Result;
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
            var topWindows = _topWindowsList.Result;
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

        #endregion

    }
}
