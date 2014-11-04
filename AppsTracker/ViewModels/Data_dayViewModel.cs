using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using AppsTracker.MVVM;
using System.Windows.Input;
using System.Collections.ObjectModel;
using AppsTracker.Utils;
using System.Diagnostics;
using AppsTracker.DAL;
using AppsTracker.DAL.Repos;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.ChartModels;
using AppsTracker.DAL.Service;

namespace AppsTracker.ViewModels
{
    internal sealed class Data_dayViewModel : ViewModelBase, IChildVM, ICommunicator
    {
        #region Fields

        DateTime _selectedDate = DateTime.Today;

        string _singleAppDuration;
        string _singleWindowDuration;
        string _duration;
        string _dayEnd;

        TopAppsModel _topAppsSingle;

        IEnumerable<TopAppsModel> _topAppsList;
        IEnumerable<DayViewModel> _dayViewModelList;
        IEnumerable<TopWindowsModel> _topWindowsList;
        IEnumerable<DailyUsageTypeSeries> _chartList;

        ICommand _singleAppSelectionChangedCommand;
        ICommand _singleWindowSelectionChangedCommand;
        ICommand _addDaysCommand;

        IAppsService _appsService;
        IChartService _chartService;

        //IRepository<Log> _logRepo;
        //IRepository<Usage> _usageRepo;

        //AppsEntities _context = new AppsEntities();

        #endregion

        #region Properties

        public string Title
        {
            get { return "DAY VIEW"; }
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
                LoadContent();
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
        public string Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                _duration = value;
                PropertyChanging("Duration");
            }
        }
        public string DayEnd
        {
            get
            {
                return _dayEnd;
            }
            set
            {
                _dayEnd = value;
                PropertyChanging("DayEnd");
            }
        }

        public bool IsContentLoaded
        {
            get;
            private set;
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
                    LoadWindowsSingle();
                PropertyChanging("TopAppsSingle");
            }
        }

        public IEnumerable<TopAppsModel> TopAppsList
        {
            get
            {
                return _topAppsList;
            }
            set
            {
                _topAppsList = value;
                PropertyChanging("TopAppsList");
            }
        }
        public IEnumerable<DayViewModel> DayViewModelList
        {
            get
            {
                return _dayViewModelList;
            }
            set
            {
                _dayViewModelList = value;
                PropertyChanging("DayViewModelList");
            }
        }
        public IEnumerable<TopWindowsModel> TopWindowsList
        {
            get
            {
                return _topWindowsList;
            }
            set
            {
                _topWindowsList = value;
                PropertyChanging("TopWindowsList");
            }
        }
        public IEnumerable<DailyUsageTypeSeries> ChartList
        {
            get
            {
                return _chartList;
            }
            set
            {
                _chartList = value;
                PropertyChanging("ChartList");
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
        }

        public async void LoadContent()
        {
            SingleAppDuration = string.Empty;
            SingleWindowDuration = string.Empty;

            var daysTask = LoadAsync(GetDayViewInfo, d => DayViewModelList = d);
            var appsTask = LoadAsync(GetTopAppsSingle, a => TopAppsList = a);
            var chartTask = LoadAsync(GetChartContent, c => ChartList = c);
            var dayInfoTask = LoadAsync(GetDayInfo, d => Duration = d);

            await Task.WhenAll(daysTask, appsTask, chartTask, dayInfoTask);

            IsContentLoaded = true;
        }
        private void LoadWindowsSingle()
        {
            Load(GetTopWindowsSingle, w => TopWindowsList = w);
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
            if (TopAppsSingle == null)
                return null;
            
            return _chartService.GetDayTopWindows(Globals.SelectedUserID, TopAppsSingle.AppName, _selectedDate);
        }

        private string GetDayInfo()
        {
            var tuple = _chartService.GetDayInfo(Globals.SelectedUserID, _selectedDate);

            return string.Format("Day start: {0}   -   Day end: {1} \t\t Total duration: {2}", tuple.Item1, tuple.Item2, tuple.Item3);
        }

        private IEnumerable<DailyUsageTypeSeries> GetChartContent()
        {
            return _chartService.GetDailySeries(Globals.SelectedUserID, _selectedDate);
        }

        #region Commmand Methods

        private void SingleAppSelectionChanged()
        {
            var topApps = TopAppsList;
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
            var topWindows = TopWindowsList;
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

        protected override void Disposing()
        {
            base.Disposing();
        }
    }
}
