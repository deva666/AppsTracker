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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Views;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class ScreenshotsViewModel : ViewModelBase, ICommunicator
    {
        private readonly IDataService dataService;
        private readonly ISqlSettingsService settingsService;

        public override string Title
        {
            get { return "SCREENSHOTS"; }
        }

        private string infoContent;
        public string InfoContent
        {
            get { return infoContent; }
            set { SetPropertyValue(ref infoContent, value); }
        }

        private DateTime selectedDate;
        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set { SetPropertyValue(ref selectedDate, value); }
        }

        private readonly AsyncProperty<IEnumerable<Log>> logList;
        public AsyncProperty<IEnumerable<Log>> LogList
        {
            get { return logList; }
        }

        private ICommand deleteSelectedScreenshotsCommand;
        public ICommand DeleteSelectedScreenshotsCommand
        {
            get { return deleteSelectedScreenshotsCommand ?? (deleteSelectedScreenshotsCommand = new DelegateCommand(DeleteSelectedScreenshots)); }
        }

        private ICommand openScreenshotViewerCommand;
        public ICommand OpenScreenshotViewerCommand
        {
            get { return openScreenshotViewerCommand ?? (openScreenshotViewerCommand = new DelegateCommand(OpenScreenshotViewer)); }
        }

        private ICommand saveScreenshotCommand;
        public ICommand SaveScreenshotCommand
        {
            get { return saveScreenshotCommand ?? (saveScreenshotCommand = new DelegateCommand(SaveScreenshot)); }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        public ScreenshotsViewModel()
        {
            dataService = ServiceFactory.Get<IDataService>();
            settingsService = ServiceFactory.Get<ISqlSettingsService>();

            logList = new AsyncProperty<IEnumerable<Log>>(GetContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(logList.Reload));
            SelectedDate = DateTime.Today;
        }

        private IEnumerable<Log> GetContent()
        {
            return dataService.GetFiltered<Log>(l => l.Screenshots.Count > 0
                                                && l.DateCreated >= Globals.DateFrom
                                                && l.DateCreated <= Globals.DateTo
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
                var deletedCount = dataService.DeleteScreenshotsInLogs(logs);
                if (deletedCount > 0)
                    InfoContent = "Screenshots deleted";
                logList.Reload();
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
                if (Directory.Exists(settingsService.Settings.DefaultScreenshotSavePath))
                    folderPath = Path.Combine(settingsService.Settings.DefaultScreenshotSavePath, Screenshots.CorrectPath(path.ToString()));
                else
                    folderPath = Screenshots.CorrectPath(path.ToString());
                Screenshots.SaveScreenshotToFile(screenshot, folderPath);
            });
        }
    }
}
