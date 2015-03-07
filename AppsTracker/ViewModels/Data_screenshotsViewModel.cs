#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Controls;
using AppsTracker.Data;
using AppsTracker.Data.Service;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Data_screenshotsViewModel : ViewModelBase, ICommunicator
    {
        #region Fields

        private IDataService _dataService;
        private ISqlSettingsService _settingsService;

        private string _infoContent;

        private DateTime _selectedDate;

        private AsyncProperty<IEnumerable<Log>> _logList;

        private ICommand _deleteSelectedScreenshotsCommand;
        private ICommand _openScreenshotViewerCommand;
        private ICommand _saveScreenshotCommand;

        #endregion

        #region Properties
        public override string Title
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
        public string InfoContent
        {
            get
            {
                return _infoContent;
            }
            set
            {
                _infoContent = value;
                PropertyChanging("InfoContent");
            }
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
            }
        }
        public Log SelectedItem { get; set; }
        public Log SelectedLog
        {
            get;
            set;
        }
        public AsyncProperty<IEnumerable<Log>> LogList
        {
            get
            {
                return _logList;
            }
            set
            {
                _logList = value;
                PropertyChanging("LogList");
            }
        }

        public ICommand DeleteSelectedScreenshotsCommand
        {
            get
            {
                if (_deleteSelectedScreenshotsCommand == null)
                    _deleteSelectedScreenshotsCommand = new DelegateCommand(DeleteSelectedScreenshots);
                return _deleteSelectedScreenshotsCommand;
            }
        }
        public ICommand OpenScreenshotViewerCommand
        {
            get
            {
                if (_openScreenshotViewerCommand == null)
                    _openScreenshotViewerCommand = new DelegateCommand(OpenScreenshotViewer);
                return _openScreenshotViewerCommand;
            }
        }
        public ICommand SaveScreenshotCommand
        {
            get
            {
                if (_saveScreenshotCommand == null)
                    _saveScreenshotCommand = new DelegateCommand(SaveScreenshot);
                return _saveScreenshotCommand;
            }
        }
        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }
        #endregion

        #region Constructor

        public Data_screenshotsViewModel()
        {
            _dataService = ServiceFactory.Get<IDataService>();
            _settingsService = ServiceFactory.Get<ISqlSettingsService>();

            _logList = new AsyncProperty<IEnumerable<Log>>(GetContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(_logList.Reload));
            SelectedDate = DateTime.Today;
        }

        #endregion

        private IEnumerable<Log> GetContent()
        {
            return _dataService.GetFiltered<Log>(l => l.Screenshots.Count > 0
                                                && l.DateCreated >= Globals.Date1
                                                && l.DateCreated <= Globals.Date2
                                                && l.Window.Application.UserID == Globals.SelectedUserID
                                                , l => l.Screenshots
                                                , l => l.Window.Application);
        }

        private void OpenScreenshotViewer(object parameter)
        {
            if (parameter == null)
                return;
            IList collection = parameter as IList;
            var logs = collection.Cast<Log>();
            ScreenshotViewerWindow window = new ScreenshotViewerWindow(logs.SelectMany(l => l.Screenshots));
            window.Owner = App.Current.MainWindow;
            window.Show();

        }

        private void DeleteSelectedScreenshots(object parameter)
        {
            ObservableCollection<object> parameterCollection = parameter as ObservableCollection<object>;
            if (parameterCollection != null)
            {
                var logs = parameterCollection.Cast<Log>().Where(l => l.Screenshots.Count > 0).ToList();
                var deletedCount = _dataService.DeleteScreenshots(logs);
                if(deletedCount > 0)
                    InfoContent = "Screenshots deleted";
                _logList.Reload();
            }
        }

        private void SaveScreenshot(object parameter)
        {
            if (parameter is ObservableCollection<object>)
                SaveAllScreenshots(parameter as ObservableCollection<object>);
            else if (parameter is Screenshot)
                SaveSingleScreenshot(parameter as Screenshot);
        }

        private async void SaveSingleScreenshot(Screenshot screenshot)
        {
            StringBuilder path = new StringBuilder();
            await SaveToFileAsync(path, screenshot.Log, screenshot);
            InfoContent = "Screenshot saved";
        }

        private async void SaveAllScreenshots(ObservableCollection<object> collection)
        {
            var logsList = collection.Cast<Log>().ToList();
            StringBuilder path = new StringBuilder();
            Working = true;
            foreach (var log in logsList)
            {
                foreach (var screenshot in log.Screenshots)
                {
                    await SaveToFileAsync(path, log, screenshot);
                    path.Clear();
                }
            }
            Working = false;
            InfoContent = "Screenshots saved";
        }

        private Task SaveToFileAsync(StringBuilder path, Log log, Screenshot screenshot)
        {
            return Task.Run(() =>
            {
                path.Append(log.Window.Application.Name);
                path.Append("_");
                path.Append(log.Window.Title);
                path.Append("_");
                path.Append(screenshot.GetHashCode());
                path.Append(".jpg");
                string folderPath;
                if (Directory.Exists(_settingsService.Settings.DefaultScreenshotSavePath))
                    folderPath = Path.Combine(_settingsService.Settings.DefaultScreenshotSavePath, Screenshots.CorrectPath(path.ToString()));
                else
                    folderPath = Screenshots.CorrectPath(path.ToString());
                Screenshots.SaveScreenshotToFile(screenshot, folderPath);
            });
        }
    }
}
