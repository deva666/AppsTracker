using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Task_Logger_Pro.MVVM;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.ChartModels;
using AppsTracker.DAL;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class Statistics_appUsageViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields
        bool _working;

        MostUsedAppModel _mostUsedAppModel;

        List<MostUsedAppModel> _mostUsedAppsList;

        List<DailyAppModel> _dailyAppList;

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


        public List<MostUsedAppModel> MostUsedAppsList
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
        public List<DailyAppModel> DailyAppList
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

        private Task<List<MostUsedAppModel>> GetContentAsync()
        {
            return Task<List<MostUsedAppModel>>.Run(new Func<List<MostUsedAppModel>>(GetContent));
        }

        private List<MostUsedAppModel> GetContent()
        {
            using (var context = new AppsEntities())
            {
                return (from u in context.Users.AsNoTracking()
                        join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                        join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                        join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                        where u.UserID == Globals.SelectedUserID
                        && l.DateCreated >= Globals.Date1
                        && l.DateCreated <= Globals.Date2
                        group l by a.Name into g
                        select g).ToList()
                                 .Select(g => new MostUsedAppModel() { AppName = g.Key, Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1) })
                                 .ToList();
            }
        }

        private Task<List<MostUsedAppModel>> LoadContentAsync()
        {
            return Task<List<MostUsedAppModel>>.Run(() =>
            {
                using (var context = new AppsEntities())
                {
                    return (from u in context.Users.AsNoTracking()
                            join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                            join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                            join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                            where u.UserID == Globals.SelectedUserID
                            && l.DateCreated >= Globals.Date1
                            && l.DateCreated <= Globals.Date2
                            group l by a.Name into g
                            select g).ToList()
                                     .Select(g => new MostUsedAppModel() { AppName = g.Key, Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1) })
                                     .ToList();
                }
            });
        }

        private async void LoadSubContent()
        {
            if (_mostUsedAppModel != null)
            {
                _dailyAppList = null;
                PropertyChanging("DailyAppList");
                Working = true;
                _dailyAppList = await LoadSubContentAsync();
                Working = false;
                PropertyChanging("DailyAppList");
            }
        }

        private Task<List<DailyAppModel>> LoadSubContentAsync()
        {
            return Task<List<DailyAppModel>>.Run(() =>
            {
                if (_mostUsedAppModel == null)
                    return null;
                var appName = MostUsedAppModel.AppName;

                using (var context = new AppsEntities())
                {
                    return (from a in context.Applications.AsNoTracking()
                            join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                            join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                            where a.Name == appName
                            group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into g
                            orderby g.Key.year, g.Key.month, g.Key.day
                            select g)
                            .ToList()
                            .Select(g => new DailyAppModel
                            {
                                Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                                Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1)
                            })
                            .ToList();
                }
            });
        }

    }
}
