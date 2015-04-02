#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;
using AppsTracker.Widgets;

namespace AppsTracker.ViewModels
{
    internal sealed class ScreenshotsViewModel : ViewModelBase, ICommunicator
    {
        private readonly IDataService dataService;
        private readonly ISqlSettingsService settingsService;
        private readonly ILoggingService loggingService;

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
            dataService = serviceResolver.Resolve<IDataService>();
            settingsService = serviceResolver.Resolve<ISqlSettingsService>();
            loggingService = serviceResolver.Resolve<ILoggingService>();

            logList = new AsyncProperty<IEnumerable<Log>>(GetContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(logList.Reload));
            SelectedDate = DateTime.Today;
        }


        private IEnumerable<Log> GetContent()
        {
            return dataService.GetFiltered<Log>(l => l.Screenshots.Count > 0
                                                && l.DateCreated >= loggingService.DateFrom
                                                && l.DateCreated <= loggingService.DateTo
                                                && l.Window.Application.UserID == loggingService.SelectedUserID
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
                    folderPath = Path.Combine(settingsService.Settings.DefaultScreenshotSavePath, CorrectPath(path.ToString()));
                else
                    folderPath = CorrectPath(path.ToString());
                SaveScreenshot(screenshot.Screensht, folderPath);
            });
        }

        private string CorrectPath(string windowTitle)
        {
            string newTitle = windowTitle;
            char[] illegalChars = new char[] { '<', '>', ':', '"', '\\', '|', '?', '*', '0' };
            if (windowTitle.IndexOfAny(illegalChars) >= 0)
            {
                foreach (var chr in illegalChars)
                {
                    if (newTitle.Contains(chr))
                    {
                        while (newTitle.Contains(chr))
                        {
                            newTitle = newTitle.Remove(newTitle.IndexOf(chr), 1);
                        }
                    }
                }
            }
            char[] charArray = newTitle.ToArray();
            foreach (var chr in charArray)
            {
                int i = chr;
                if (i >= 1 && i <= 31) newTitle = newTitle.Remove(newTitle.IndexOf(chr), 1);
            }

            newTitle = TrimPath(newTitle);
            return newTitle;
        }

        private string TrimPath(string path)
        {
            if (path.Length >= 247)
            {
                while (path.Length >= 247)
                {
                    path = path.Remove(path.Length - 1, 1);
                }
            }
            return path;
        }

        private void SaveScreenshot(byte[] image, string path)
        {
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

            try
            {
                using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate))
                {
                    fileStream.Write(image, 0, image.Length);
                }
            }
            catch (IOException ex)
            {
                serviceResolver.Resolve<IMessageService>().ShowDialog(ex);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
