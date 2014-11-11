using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using AppsTracker.DAL.Repos;
using AppsTracker.Models.ChartModels;
using AppsTracker.MVVM;
using AppsTracker.DAL.Service;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Statistics_screenshotsViewModel : ViewModelBase, IChildVM, ICommunicator
    {
        #region Fields

        ICommand _returnFromDetailedViewCommand;

        ScreenshotModel _screenshotModel;

        IEnumerable<ScreenshotModel> _screenshotList;

        IEnumerable<DailyScreenshotModel> _dailyScreenshotsList;

        IChartService _service;

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

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        public Statistics_screenshotsViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
            _service = ServiceFactory.Get<IChartService>();
        }

        public async void LoadContent()
        {
            await LoadAsync(GetContent, s => ScreenshotList = s);
            if (ScreenshotModel != null)
                LoadSubContent();
            IsContentLoaded = true;
        }

        private IEnumerable<ScreenshotModel> GetContent()
        {
            return _service.GetScreenshots(Globals.SelectedUserID, Globals.Date1, Globals.Date2);
        }

        private IEnumerable<DailyScreenshotModel> GetSubContent()
        {
            var model = ScreenshotModel;
            if (model == null)
                return null;
            return _service.GetScreenshotsByApp(Globals.SelectedUserID, model.AppName, Globals.Date1, Globals.Date2);
        }

        private async Task<IEnumerable<ScreenshotModel>> GetContentAsync()
        {
            var screenshots = await ScreenshotRepo.Instance.GetAsync(s => s.Log.Window.Application, s => s.Log.Window.Application.User).ConfigureAwait(false);

            var filtered = screenshots.Where(s => s.Log.Window.Application.User.UserID == Globals.SelectedUserID
                                                && s.Date >= Globals.Date1
                                                && s.Date <= Globals.Date2)
                                            .GroupBy(s => s.Log.Window.Application.Name)
                                            .Select(g => new ScreenshotModel() { AppName = g.Key, Count = g.Count() });

            return filtered;
        }

        private async void LoadSubContent()
        {
            DailyScreenshotsList = null;
            await LoadAsync(GetSubContent, d => DailyScreenshotsList = d);
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
                                        .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

            return grouped.Select(g => new DailyScreenshotModel() { Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(), Count = g.Count() });
        }

        private void ReturnFromDetailedView()
        {
            ScreenshotModel = null;
        }
    }
}
