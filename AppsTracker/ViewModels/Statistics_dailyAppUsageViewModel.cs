using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.DAL;
using AppsTracker.DAL.Repos;
using AppsTracker.Models.ChartModels;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class Statistics_dailyAppUsageViewModel : ViewModelBase, IChildVM, IWorker, ICommunicator
    {
        #region Fields

        bool _working;

        IEnumerable<DailyUsedAppsSeries> _dailyUsedAppsList;

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

        public IEnumerable<DailyUsedAppsSeries> DailyUsedAppsList
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
            DailyUsedAppsList = await GetContentAsync().ConfigureAwait(false);
            Working = false;
            IsContentLoaded = true;
        }

        private Task<List<DailyUsedAppsSeries>> GetContentAsync()
        {
            return Task<List<DailyUsedAppsSeries>>.Run(() =>
            {
                List<DailyUsedAppsSeries> dailyUsedAppsSeriesTemp = new List<DailyUsedAppsSeries>();

                var logs =  LogRepo.Instance.Get(l => l.Window.Application, l => l.Window.Application.User);

                var grouped = logs.Where(l => l.Window.Application.User.UserID == Globals.SelectedUserID
                                              && l.DateCreated >= Globals.Date1
                                              && l.DateCreated <= Globals.Date2)
                                    .OrderBy(l => l.DateCreated)
                                    .GroupBy(l => new
                                                    {
                                                        year = l.DateCreated.Year,
                                                        month = l.DateCreated.Month,
                                                        day = l.DateCreated.Day,
                                                        name = l.Window.Application.Name
                                                    });

                var dailyApps = grouped.Select(g => new
                                                        {
                                                            Date = new DateTime(g.Key.year, g.Key.month, g.Key.day),
                                                            AppName = g.Key.name,
                                                            Duration = g.Sum(l => l.Duration)
                                                        });

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


                foreach (var item in dailyUsedAppsSeriesTemp)
                    item.DailyUsedAppsCollection = item.DailyUsedAppsCollection.OrderBy(d => d.Duration).ToList();


                return dailyUsedAppsSeriesTemp;
            });

        }              
    }
}
