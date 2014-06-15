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
    class Statistics_keystrokesViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        ICommand _returnFromDetailedViewCommand;

        KeystrokeModel _keystrokeModel;

        List<KeystrokeModel> _keystrokeList;

        List<DailyKeystrokeModel> _dailyKeystrokesList;

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return "KEYSTROKES";
            }
        }

        public bool IsContentLoaded
        {
            get;
            private set;
        }

        public object SelectedItem
        {
            get;
            set;
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

        public ICommand ReturnFromDetailedViewCommand
        {
            get
            {
                if (_returnFromDetailedViewCommand == null)
                    _returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView);
                return _returnFromDetailedViewCommand;
            }
        }

        public KeystrokeModel KeystrokeModel
        {
            get
            {
                return _keystrokeModel;
            }
            set
            {
                _keystrokeModel = value;
                PropertyChanging("KeystrokeModel");
                if (_keystrokeModel != null)
                    DailyKeystrokesList = LoadSubContentAsync().Result;
            }
        }

        public List<KeystrokeModel> KeystrokeList
        {
            get
            {
                return _keystrokeList;
            }
            set
            {
                _keystrokeList = value;
                PropertyChanging("KeystrokeList");
            }
        }
        public List<DailyKeystrokeModel> DailyKeystrokesList
        {
            get
            {
                return _dailyKeystrokesList;
            }
            set
            {
                _dailyKeystrokesList = value;
                PropertyChanging("DailyKeystrokesList");
            }
        }

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }

        #endregion

        public Statistics_keystrokesViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
        }

        public async void LoadContent()
        {
            Working = true;
            KeystrokeList = await LoadContentAsync();
            if (KeystrokeModel != null)
                DailyKeystrokesList = await LoadSubContentAsync();
            Working = false;
            IsContentLoaded = true;
        }

        private Task<List<KeystrokeModel>> LoadContentAsync()
        {
            return Task<List<KeystrokeModel>>.Run(() =>
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
                            && l.Keystrokes != null
                            group l by a.Name into g
                            select g)
                            .ToList()
                            .Select(g => new KeystrokeModel { AppName = g.Key, Count = g.Sum(l => l.Keystrokes.Length) })
                            .ToList();
                }
            });
        }

        private async Task LoadSubContent()
        {
            if (KeystrokeModel == null)
                return;
            _dailyKeystrokesList = null;
            PropertyChanging("DailyKeystrokesCollection");
            Working = true;
            _dailyKeystrokesList = await LoadSubContentAsync();
            Working = false;
            PropertyChanging("DailyKeystrokesCollection");
        }

        private Task<List<DailyKeystrokeModel>> LoadSubContentAsync()
        {
            return Task<List<DailyKeystrokeModel>>.Run(() =>
            {
                using (var context = new AppsEntities())
                {
                    return (from u in context.Users
                            join a in context.Applications on u.UserID equals a.UserID
                            join w in context.Windows on a.ApplicationID equals w.ApplicationID
                            join l in context.Logs on w.WindowID equals l.WindowID
                            where u.UserID == Globals.SelectedUserID
                            && l.DateCreated >= Globals.Date1
                            && l.DateCreated <= Globals.Date2
                            && l.Keystrokes != null
                            && a.Name == KeystrokeModel.AppName
                            group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into g
                            orderby g.Key.year, g.Key.month, g.Key.day
                            select g).ToList()
                                    .Select(g => new DailyKeystrokeModel()
                                    {
                                        Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString()
                                        ,
                                        Count = g.Sum(l => l.Keystrokes.Length)
                                    })
                                    .ToList();
                }
            });
        }

        private void ReturnFromDetailedView()
        {
            KeystrokeModel = null;
        }
    }
}
