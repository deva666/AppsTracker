using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.Logging;
using Task_Logger_Pro.MVVM;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;

namespace Task_Logger_Pro.Pages.ViewModels
{
    public class Data_screenshotsViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        string _infoContent;

        DateTime _selectedDate;

        List<Log> _logList;

        ICommand _deleteSelectedLogsCommand;
        ICommand _openScreenshotViewerCommand;
        ICommand _saveScreenshotCommand;

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
        public List<Log> LogList
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

        public ICommand DeleteSelectedLogsCommand
        {
            get
            {
                if (_deleteSelectedLogsCommand == null)
                    _deleteSelectedLogsCommand = new DelegateCommand(DeleteSelectedLogs);
                return _deleteSelectedLogsCommand;
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
        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }
        #endregion

        #region Constructor

        public Data_screenshotsViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
            //Mediator.Register(MediatorMessages.ScreenshotAdded, new Action<object>((p) => { if (this.WeakCollection.Target != null) LoadContent(); }));
            SelectedDate = DateTime.Today;
        }

        #endregion

        public async void LoadContent()
        {
            Working = true;
            LogList = await LoadContentAsync();
            Working = false;
            IsContentLoaded = true;
        }

        private Task<List<Log>> LoadContentAsync()
        {
            return Task<List<Log>>.Factory.StartNew(() =>
            {
                using (var context = new AppsEntities())
                {
                    return (from u in context.Users.AsNoTracking()
                            join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                            join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                            join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                            where u.UserID == Globals.SelectedUserID
                            && l.Screenshots.Count > 0
                            && l.DateCreated >= Globals.Date1
                            && l.DateCreated <= Globals.Date2
                            orderby l.DateCreated
                            select l).Include(w => w.Window.Application)
                                    .Include(l => l.Screenshots)
                                    .ToList();
                }
            });
        }

        //private void ScreenshotAdded(Log log)
        //{
        //    if (_weakCollection.Target == null)
        //        return;
        //    List<Log> logList = (_weakCollection.Target as List<Log>).ToList();
        //    if (logList == null)
        //        return;
        //    if (logList.Any(l => l.LogID == log.LogID))
        //    {
        //        var selectedLog = logList.Find(l => l.LogID == log.LogID);
        //        foreach (var screenshot in log.Screenshots)
        //        {
        //            if (!selectedLog.Screenshots.Contains(screenshot))
        //            {
        //                selectedLog.Screenshots.Add(screenshot);
        //            }
        //        }
        //    }
        //    else
        //        logList.Add(log);
        //    _weakCollection.Target = logList;
        //    PropertyChanging("WeakCollection");
        //}

        //private List<ScreenshotApp> GetAppsOnSelectedDate()
        //{
        //    using (var context = new AppsEntities())
        //    {
        //        DateTime dateEnd = SelectedDate.AddDays(1);
        //        return (from u in context.Users.AsNoTracking()
        //                join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
        //                join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
        //                join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
        //                where u.UserID == Globals.SelectedUserID
        //                && l.Screenshots.Count > 0
        //                && l.DateCreated >= SelectedDate
        //                && l.DateCreated <= dateEnd
        //                select a).Distinct()
        //                        .ToList()
        //                        .Select(a => new ScreenshotApp() { Name = a.Name, IsSelected = true })
        //                        .ToList();
        //    }

        //}

        //private void SelectLogs()
        //{
        //    if (ScreenshotAppList == null)
        //        return;
        //    ClearSelection();
        //    if (LogList == null)
        //        return;
        //    foreach (var app in ScreenshotAppList)
        //    {
        //        if (!app.IsSelected)
        //            continue;
        //        var logs = from l in LogList
        //                   where l.Window.Application.Name == app.Name
        //                   && l.DateCreated >= SelectedDate
        //                   && l.DateCreated <= SelectedDate.AddDays(1)
        //                   select l;
        //        foreach (var log in logs)
        //        {
        //            log.IsSelected = true;
        //        }

        //    }
        //    PropertyChanging("WeakCollection");
        //}

        private void SortViewProcesses(object parameter)
        {
            if (LogList == null)
                return;
            string propertyName = parameter as string;
            ICollectionView view;
            switch (propertyName)
            {
                case "DateCreated":
                    view = CollectionViewSource.GetDefaultView(LogList);
                    break;
                default:
                    view = null;
                    break;
            }

            if (view != null)
            {
                if (view.SortDescriptions.Count > 0 && view.SortDescriptions[0].PropertyName == propertyName && view.SortDescriptions[0].Direction == ListSortDirection.Ascending)
                {
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Descending));
                }
                else
                {
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Ascending));
                }
            }
        }

        private void ClearSelection()
        {
            if (LogList == null)
                return;
            foreach (var log in LogList.Where(l => l.IsSelected))
            {
                log.IsSelected = false;
            }
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

        private void DeleteSelectedLogs(object parameter)
        {
            ObservableCollection<object> parameterCollection = parameter as ObservableCollection<object>;
            if (parameterCollection != null)
            {
                var logsList = parameterCollection.Select(l => l as Log).ToList();
                using (var context = new AppsEntities())
                {
                    foreach (var log in logsList)
                    {
                        foreach (var screenshot in log.Screenshots.ToList())
                        {
                            if (!context.Screenshots.Local.Any(s => s.ScreenshotID == screenshot.ScreenshotID))
                            {
                                context.Screenshots.Attach(screenshot);
                            }
                            context.Screenshots.Remove(screenshot);
                        }
                        if (!context.Logs.Local.Any(l => l.LogID == log.LogID))
                        {
                            context.Logs.Attach(log);
                        }
                        context.Entry(log).State = System.Data.Entity.EntityState.Deleted;
                    }
                    context.SaveChanges();
                }
                if (logsList.Count > 0)
                    LoadContent();
                InfoContent = "Logs deleted";
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
            await SaveToFile(path, screenshot.Log, screenshot);
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
                    await SaveToFile(path, log, screenshot);
                    path.Clear();
                }
            }
            Working = false;
            InfoContent = "Screenshots saved";
        }

        private Task SaveToFile(StringBuilder path, Log log, Screenshot screenshot)
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
                if (Directory.Exists(App.UzerSetting.DefaultScreenshotSavePath))
                    folderPath = Path.Combine(App.UzerSetting.DefaultScreenshotSavePath, Screenshots.CorrectPath(path.ToString()));
                else
                    folderPath = Screenshots.CorrectPath(path.ToString());
                Screenshots.SaveScreenshotToFileAsync(screenshot, folderPath);
            });
        }
    }
}
