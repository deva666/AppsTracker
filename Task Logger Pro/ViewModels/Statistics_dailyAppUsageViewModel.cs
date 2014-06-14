using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.DAL;
using AppsTracker.Models.ChartModels;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class Statistics_dailyAppUsageViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        List<DailyUsedAppsSeries> _dailyUsedAppsList;
        
        #endregion

        #region Properties
        
        public string Title
        {
            get
            {
                return "DAILY APP USAGE";
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

        public List<DailyUsedAppsSeries> DailyUsedAppsList
        {
            get
            {
                return _dailyUsedAppsList;
            }
            set
            {
                _dailyUsedAppsList = value;
                PropertyChanging("DailyUsedAppsList");
            }
        }

 
        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        } 

        #endregion

        public Statistics_dailyAppUsageViewModel()
        {
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
        }

        public async void LoadContent()
        {
            Working = true;
            DailyUsedAppsList = await LoadContentAsync();
            Working = false;
            IsContentLoaded = true;
        }

        private Task<List<DailyUsedAppsSeries>> LoadContentAsync()
        {
            return Task<List<DailyUsedAppsSeries>>.Factory.StartNew(() =>
             {
                 List<DailyUsedAppsSeries> dailyUsedAppsSeriesTemp = new List<DailyUsedAppsSeries>();
                 using (var context = new AppsEntities())
                 {

                     var dailyApps = (from u in context.Users
                                      join a in context.Applications on u.UserID equals a.UserID
                                      join w in context.Windows on a.ApplicationID equals w.ApplicationID
                                      join l in context.Logs on w.WindowID equals l.WindowID
                                      where u.UserID == Globals.SelectedUserID
                                      && l.DateCreated >= Globals.Date1
                                      && l.DateCreated <= Globals.Date2
                                      group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day, name = a.Name } into g
                                      select g).ToList()
                                      .Select(g => new { Date = new DateTime(g.Key.year, g.Key.month, g.Key.day), AppName = g.Key.name, Duration = g.Sum(l => l.Duration) });
                                      //.OrderByDescending(g=>g.Duration);

                     List<MostUsedAppModel> dailyUsedAppsCollection;
                     foreach (var app in dailyApps)
                     {
                         if (app.Duration > 0)
                         {
                             if (!dailyUsedAppsSeriesTemp.Exists(d => d.Date == app.Date.ToShortDateString()))
                             {
                                 dailyUsedAppsCollection = new List<MostUsedAppModel>();
                                 dailyUsedAppsCollection.Add(new MostUsedAppModel() { AppName = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
                                 dailyUsedAppsSeriesTemp.Add(new DailyUsedAppsSeries() { Date = app.Date.ToShortDateString(), DailyUsedAppsCollection = dailyUsedAppsCollection });
                             }
                             else
                             {
                                 dailyUsedAppsSeriesTemp.First(d => d.Date == app.Date.ToShortDateString())
                                     .DailyUsedAppsCollection.Add(new MostUsedAppModel() { AppName = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
                             }
                         }
                     }
                 }

                 foreach (var item in dailyUsedAppsSeriesTemp)
                 {
                     item.DailyUsedAppsCollection = item.DailyUsedAppsCollection.OrderBy(d => d.Duration).ToList();
                 }

                 return dailyUsedAppsSeriesTemp;
             }, System.Threading.CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
