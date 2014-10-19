using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Task_Logger_Pro.MVVM;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.ChartModels;
using AppsTracker.DAL;
using System.Diagnostics;
using AppsTracker.DAL.Repos;


namespace Task_Logger_Pro.Pages.ViewModels
{
    class Statistics_appUsageViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields
        bool _working;

        MostUsedAppModel _mostUsedAppModel;

        IEnumerable<MostUsedAppModel> _mostUsedAppsList;

        IEnumerable<DailyAppModel> _dailyAppList;

        ICommand _returnFromDetailedViewCommand;

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return "APPS";
            }
        }

        public bool IsContentLoaded
        {
            get;
            private set;
        }

        public bool Working
        {
            get
            {
                return _working;
            }
            set
            {
                _working = value;
                PropertyChanging("Working");
            }
        }

        public MostUsedAppModel MostUsedAppModel
        {
            get
            {
                return _mostUsedAppModel;
            }
            set
            {
                _mostUsedAppModel = value;
                PropertyChanging("MostUsedAppModel");
                LoadSubContent();
            }
        }

        public object SelectedItem
        {
            get;
            set;
        }


        public IEnumerable<MostUsedAppModel> MostUsedAppsList
        {
            get
            {
                return _mostUsedAppsList;
            }
            set
            {
                _mostUsedAppsList = value;
                PropertyChanging("MostUsedAppsList");
            }
        }
        public IEnumerable<DailyAppModel> DailyAppList
        {
            get
            {
                return _dailyAppList;
            }
            set
            {
                _dailyAppList = value;
                PropertyChanging("DailyAppList");
            }
        }
        public ICommand ReturnFromDetailedViewCommand
        {
            get
            {
                return _returnFromDetailedViewCommand == null ? _returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView) : _returnFromDetailedViewCommand;
            }
        }

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }

        #endregion

        private void ReturnFromDetailedView()
        {
            MostUsedAppModel = null;
        }

        public Statistics_appUsageViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
        }

        public async void LoadContent()
        {
            Working = true;
            MostUsedAppsList = await GetContentAsync();
            LoadSubContent();
            Working = false;
            IsContentLoaded = true;
        }

        private async Task<IEnumerable<MostUsedAppModel>> GetContentAsync()
        {
            var logs = await LogRepo.Instance.GetAsync(l => l.Window.Application, l => l.Window.Application.User).ConfigureAwait(false);

            var grouped = logs.Where(l => l.Window.Application.User.UserID == Globals.SelectedUserID
                                        && l.DateCreated >= Globals.Date1
                                        && l.DateCreated <= Globals.Date2)
                                .GroupBy(l => l.Window.Application.Name);

            return grouped.Select(g => new MostUsedAppModel()
                                                            {
                                                                AppName = g.Key,
                                                                Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1)
                                                            });
        }

        private async void LoadSubContent()
        {
            if (_mostUsedAppModel == null)
                return;

            DailyAppList = null;
            Working = true;
            DailyAppList = await LoadSubContentAsync();
            Working = false;
        }

        private async Task<IEnumerable<DailyAppModel>> LoadSubContentAsync()
        {
            if (_mostUsedAppModel == null)
                return null;
            var appName = MostUsedAppModel.AppName;

            var logs = await LogRepo.Instance.GetFilteredAsync(l => l.Window.Application.Name == appName
                                                                && l.Window.Application.User.UserID == Globals.SelectedUserID
                                                                && l.DateCreated >= Globals.Date1
                                                                && l.DateCreated <= Globals.Date2)
                                                .ConfigureAwait(false);

            var grouped = logs.GroupBy(l => new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day })
                                .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

            return grouped.Select(g => new DailyAppModel
                            {
                                Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                                Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1)
                            });

        }
    }
}
