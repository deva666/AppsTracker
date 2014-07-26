using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.DAL;
using AppsTracker.Models.ChartModels;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class Statistics_screenshotsViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        ICommand _returnFromDetailedViewCommand;

        ScreenshotModel _screenshotModel;

        List<ScreenshotModel> _screenshotList;

        List<DailyScreenshotModel> _dailyScreenshotsList;

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return "SCREENSHOTS";
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

        public object SelectedItem
        {
            get;
            set;
        }

        public ICommand ReturnFromDetailedViewCommand
        {
            get
            {
                return _returnFromDetailedViewCommand == null ? _returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView) : _returnFromDetailedViewCommand;
            }
        }

        public ScreenshotModel ScreenshotModel
        {
            get
            {
                return _screenshotModel;
            }
            set
            {
                _screenshotModel = value;
                PropertyChanging("ScreenshotModel");
                if (_screenshotModel != null)
                    LoadSubContent();
            }
        }

        public List<ScreenshotModel> ScreenshotList
        {
            get
            {
                return _screenshotList;
            }
            set
            {
                _screenshotList = value;
                PropertyChanging("ScreenshotList");
            }
        }

        public List<DailyScreenshotModel> DailyScreenshotsList
        {
            get
            {
                return _dailyScreenshotsList;
            }
            set
            {
                _dailyScreenshotsList = value;
                PropertyChanging("DailyScreenshotsList");
            }
        }

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }

        #endregion

        public Statistics_screenshotsViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
        }

        public async void LoadContent()
        {
            Working = true;
            ScreenshotList = await GetContentAsync();
            if (ScreenshotModel != null)
                DailyScreenshotsList = await LoadSubContentAsync();
            Working = false;
            IsContentLoaded = true;
        }

        private List<ScreenshotModel> GetContent()
        {
            using (var context = new AppsEntities())
            {
                return (from u in context.Users
                        join a in context.Applications on u.UserID equals a.UserID
                        join w in context.Windows on a.ApplicationID equals w.ApplicationID
                        join l in context.Logs on w.WindowID equals l.WindowID
                        join s in context.Screenshots on l.LogID equals s.LogID
                        where u.UserID == Globals.SelectedUserID
                        && s.Date >= Globals.Date1
                        && s.Date <= Globals.Date2
                        group s by a.Name into g
                        select g)
                            .ToList()
                            .Select(g => new ScreenshotModel() { AppName = g.Key, Count = g.Count() })
                            .ToList();
            }
        }

        private Task<List<ScreenshotModel>> GetContentAsync()
        {
            return Task<List<ScreenshotModel>>.Run(new Func<List<ScreenshotModel>>(GetContent));
        }

        private Task<List<ScreenshotModel>> LoadContentAsync()
        {
            return Task<List<ScreenshotModel>>.Run(() =>
            {
                using (var context = new AppsEntities())
                {
                    return (from u in context.Users
                            join a in context.Applications on u.UserID equals a.UserID
                            join w in context.Windows on a.ApplicationID equals w.ApplicationID
                            join l in context.Logs on w.WindowID equals l.WindowID
                            join s in context.Screenshots on l.LogID equals s.LogID
                            where u.UserID == Globals.SelectedUserID
                            && s.Date >= Globals.Date1
                            && s.Date <= Globals.Date2
                            group s by a.Name into g
                            select g)
                                .ToList()
                                .Select(g => new ScreenshotModel() { AppName = g.Key, Count = g.Count() })
                                .ToList();
                }
            });
        }

        private async void LoadSubContent()
        {
            if (ScreenshotModel != null)
            {
                _dailyScreenshotsList = null;
                PropertyChanging("DailyScreenshotsList");
                Working = true;
                _dailyScreenshotsList = await LoadSubContentAsync();
                Working = false;
                PropertyChanging("DailyScreenshotsList");
            }
        }

        private Task<List<DailyScreenshotModel>> LoadSubContentAsync()
        {
            return Task<List<DailyScreenshotModel>>.Factory.StartNew(() =>
            {
                using (var context = new AppsEntities())
                {
                    return (from u in context.Users
                            join a in context.Applications on u.UserID equals a.UserID
                            join w in context.Windows on a.ApplicationID equals w.ApplicationID
                            join l in context.Logs on w.WindowID equals l.WindowID
                            join s in context.Screenshots on l.LogID equals s.LogID
                            where u.UserID == Globals.SelectedUserID
                            && s.Date >= Globals.Date1
                            && s.Date <= Globals.Date2
                            && a.Name == ScreenshotModel.AppName
                            group s by new { year = s.Date.Year, month = s.Date.Month, day = s.Date.Day } into g
                            orderby g.Key.year, g.Key.month, g.Key.day
                            select g).ToList()
                                    .Select(g => new DailyScreenshotModel() { Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(), Count = g.Count() })
                                    .ToList();
                }
            });
        }

        private void ReturnFromDetailedView()
        {
            ScreenshotModel = null;
        }
    }
}
