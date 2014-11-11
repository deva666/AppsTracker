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
using AppsTracker.Controls;
using AppsTracker.MVVM;
using AppsTracker.DAL.Service;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Statistics_keystrokesViewModel : ViewModelBase, IChildVM, ICommunicator
    {
        #region Fields

        ICommand _returnFromDetailedViewCommand;

        KeystrokeModel _keystrokeModel;

        IEnumerable<KeystrokeModel> _keystrokeList;

        IEnumerable<DailyKeystrokeModel> _dailyKeystrokesList;

        IChartService _service;

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

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        public Statistics_keystrokesViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
            _service = ServiceFactory.Get<IChartService>();
        }

        public async void LoadContent()
        {
            await LoadAsync(GetContent, k => KeystrokeList = k);
            if (_keystrokeModel != null)
                LoadSubContent();
            IsContentLoaded = true;
        }

        private IEnumerable<KeystrokeModel> GetContent()
        {
            return _service.GetKeystrokes(Globals.SelectedUserID, Globals.Date1, Globals.Date2);
        }

        private Task<List<KeystrokeModel>> GetContentAsync()
        {
            return Task<List<KeystrokeModel>>.Run(() =>
            {
                var logs = LogRepo.Instance.Get(l => l.Window.Application, l => l.Window.Application.User);

                var filtered = logs.Where(l => l.Window.Application.User.UserID == Globals.SelectedUserID
                                                                    && l.DateCreated >= Globals.Date1
                                                                    && l.DateCreated <= Globals.Date2
                                                                    && l.Keystrokes != null);
                return filtered
                        .GroupBy(l => l.Window.Application.Name)
                        .Select(g => new KeystrokeModel() { AppName = g.Key, Count = g.Sum(l => l.Keystrokes.Length) })
                        .OrderByDescending(k => k.Count)
                        .ToList();
            });
        }

        private async void LoadSubContent()
        {
            DailyKeystrokesList = null;
            await LoadAsync(GetSubContent, d => DailyKeystrokesList = d);
        }

        IEnumerable<DailyKeystrokeModel> GetSubContent()
        {
            if (KeystrokeModel == null)
                return null;

            return _service.GetKeystrokesByApp(Globals.SelectedUserID, KeystrokeModel.AppName, Globals.Date1, Globals.Date2);
        }

        private Task<IEnumerable<DailyKeystrokeModel>> GetSubContentAsync()
        {
            return Task<IEnumerable<DailyKeystrokeModel>>.Run(() =>
             {
                 var logs = LogRepo.Instance.Get(l => l.Window.Application, l => l.Window.Application.User);

                 var filtered = logs.Where(l => l.Window.Application.User.UserID == Globals.SelectedUserID
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
             });
        }

        private void ReturnFromDetailedView()
        {
            KeystrokeModel = null;
        }
    }
}
