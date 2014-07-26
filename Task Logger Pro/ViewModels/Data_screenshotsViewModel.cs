﻿using System;
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
    class Data_screenshotsViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        string _infoContent;

        DateTime _selectedDate;

        List<Log> _logList;

        ICommand _deleteSelectedScreenshotsCommand;
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
        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }
        #endregion

        #region Constructor

        public Data_screenshotsViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));            
            SelectedDate = DateTime.Today;
        }

        #endregion

        public async void LoadContent()
        {
            Working = true;
            LogList = await GetContentAsync();
            Working = false;
            IsContentLoaded = true;
        }

        private Task<List<Log>> GetContentAsync()
        {
            return Task<List<Log>>.Run(new Func<List<Log>>(GetContent));
        }

        private List<Log> GetContent()
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
                var logsList = parameterCollection.Cast<Log>().Where(l => l.Screenshots.Count > 0).ToList();
                var count = logsList.Select(l => l.Screenshots).Count();
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
                    }
                    context.SaveChanges();
                }
                if (count > 0)
                    InfoContent = "Screenshots deleted";
                LoadContent();
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
                if (Directory.Exists(App.UzerSetting.DefaultScreenshotSavePath))
                    folderPath = Path.Combine(App.UzerSetting.DefaultScreenshotSavePath, Screenshots.CorrectPath(path.ToString()));
                else
                    folderPath = Screenshots.CorrectPath(path.ToString());
                Screenshots.SaveScreenshotToFileAsync(screenshot, folderPath);
            });
        }
    }
}
