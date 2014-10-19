using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.DAL;
using AppsTracker.DAL.Repos;
using AppsTracker.Models.ChartModels;
using AppsTracker.Controls;
using AppsTracker.MVVM;

namespace AppsTracker.Pages.ViewModels
{
    class Statistics_screenshotsViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        ICommand _returnFromDetailedViewCommand;

        ScreenshotModel _screenshotModel;

        IEnumerable<ScreenshotModel> _screenshotList;

        IEnumerable<DailyScreenshotModel> _dailyScreenshotsList;

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

        public IEnumerable<ScreenshotModel> ScreenshotList
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

        public IEnumerable<DailyScreenshotModel> DailyScreenshotsList
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

        private async Task<IEnumerable<ScreenshotModel>> GetContentAsync()
        {
            var screenshots = await ScreenshotRepo.Instance.GetAsync(s => s.Log.Window.Application , s => s.Log.Window.Application.User).ConfigureAwait(false);

            var filtered = screenshots.Where(s => s.Log.Window.Application.User.UserID == Globals.SelectedUserID
                                                && s.Date >= Globals.Date1
                                                && s.Date <= Globals.Date2)
                                            .GroupBy(s => s.Log.Window.Application.Name)
                                            .Select(g => new ScreenshotModel() { AppName = g.Key, Count = g.Count() });
                            
            return filtered;
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

        private async Task<IEnumerable<DailyScreenshotModel>> LoadSubContentAsync()
        {
            var screenshots = await ScreenshotRepo.Instance.GetFilteredAsync(s => s.Log.Window.Application.User.UserID == Globals.SelectedUserID
                                                                                && s.Date >= Globals.Date1
                                                                                && s.Date <= Globals.Date2
                                                                                && s.Log.Window.Application.Name == ScreenshotModel.AppName
                                                                                )
                                                            .ConfigureAwait(false);

            var grouped = screenshots.GroupBy(s => new { year = s.Date.Year, month = s.Date.Month, day = s.Date.Day })
                                        .OrderBy(g=> new DateTime(g.Key.year, g.Key.month,g.Key.day));
            
            return grouped.Select(g => new DailyScreenshotModel() { Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(), Count = g.Count() });
        }

        private void ReturnFromDetailedView()
        {
            ScreenshotModel = null;
        }
    }
}
