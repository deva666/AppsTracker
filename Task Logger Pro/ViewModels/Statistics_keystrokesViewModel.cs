using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.DAL;
using AppsTracker.DAL.Repos;
using AppsTracker.Models.ChartModels;
using AppsTracker.Models.EntityModels;
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

        IEnumerable<KeystrokeModel> _keystrokeList;

        IEnumerable<DailyKeystrokeModel> _dailyKeystrokesList;

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
                return _returnFromDetailedViewCommand == null ? _returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView) : _returnFromDetailedViewCommand;
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
                    LoadSubContent();
            }
        }

        public IEnumerable<KeystrokeModel> KeystrokeList
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
        public IEnumerable<DailyKeystrokeModel> DailyKeystrokesList
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
            KeystrokeList = await GetContentFromRepo();
            if (KeystrokeModel != null)
                DailyKeystrokesList = await GetSubContentFromRepo();
            Working = false;
            IsContentLoaded = true;
        }

        private async Task<IEnumerable<KeystrokeModel>> GetContentFromRepo()
        {
            var logs = await LogRepo.Instance.GetAsync(l => l.Window.Application, l => l.Window.Application.User).ConfigureAwait(false);

            var filtered = logs.Where(l => l.Window.Application.User.UserID == Globals.SelectedUserID
                                                                && l.DateCreated >= Globals.Date1
                                                                && l.DateCreated <= Globals.Date2
                                                                && l.Keystrokes != null);
            return filtered
                    .GroupBy(l => l.Window.Application.Name)
                    .Select(g => new KeystrokeModel() { AppName = g.Key, Count = g.Sum(l => l.Keystrokes.Length) })
                    .OrderByDescending(k => k.Count)
                    .ToList();

        }

        private async void LoadSubContent()
        {
            if (KeystrokeModel == null)
                return;
            DailyKeystrokesList = null;
            Working = true;
            DailyKeystrokesList = await GetSubContentFromRepo();
            Working = false;
        }

        private async Task<IEnumerable<DailyKeystrokeModel>> GetSubContentFromRepo()
        {
            var logs = await LogRepo.Instance.GetAsync(l => l.Window.Application, l => l.Window.Application.User).ConfigureAwait(false);

            var filtered = logs.Where(l=>l.Window.Application.User.UserID == Globals.SelectedUserID
                                         && l.DateCreated >= Globals.Date1
                                         && l.DateCreated <= Globals.Date2
                                         && l.Keystrokes != null
                                         && l.Window.Application.Name == KeystrokeModel.AppName);

            return filtered.GroupBy(l => new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day })
                        .Select(g => new DailyKeystrokeModel()
                        {
                            Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                            Count = g.Sum(l => l.Keystrokes.Length)
                        });
        }

        private void ReturnFromDetailedView()
        {
            KeystrokeModel = null;
        }
    }
}
