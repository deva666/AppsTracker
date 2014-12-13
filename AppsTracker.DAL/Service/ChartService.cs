#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Collections.ObjectModel;

using AppsTracker.Models.EntityModels;
using AppsTracker.Models.ChartModels;
using AppsTracker.Models.Utils;

namespace AppsTracker.DAL.Service
{
    public class ChartService : IChartService
    {
        private object @parallelLock = new object();

        private IEnumerable<Log> _cachedLogs = null;

        public IEnumerable<TopAppsModel> GetLogTopApps(int userID, int appID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {

                var logs = context.Logs.Where(l => l.Window.Application.User.UserID == userID
                                                && l.DateCreated >= dateFrom
                                                && l.DateCreated <= dateTo)
                                        .Include(l => l.Window.Application)
                                        .ToList();

                var totalDuration = (from l in logs
                                     group l by new
                                     {
                                         year = l.DateCreated.Year,
                                         month = l.DateCreated.Month,
                                         day = l.DateCreated.Day
                                     } into grp
                                     select grp)
                                    .ToList()
                                    .Select(g => new
                                    {
                                        Date = new DateTime(g.Key.year, g.Key.month, g.Key.day),
                                        Duration = (double)g.Sum(l => l.Duration)
                                    });


                var result = (from l in logs
                              where l.Window.Application.ApplicationID == appID
                              group l by new
                              {
                                  year = l.DateCreated.Year,
                                  month = l.DateCreated.Month,
                                  day = l.DateCreated.Day,
                                  name = l.Window.Application.Name
                              } into grp
                              select grp)
                              .ToList()
                              .Select(g => new TopAppsModel
                                   {
                                       AppName = g.Key.name,
                                       Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                                       DateTime = new DateTime(g.Key.year, g.Key.month, g.Key.day),
                                       Usage = g.Sum(l => l.Duration) / totalDuration.First(t => t.Date == new DateTime(g.Key.year, g.Key.month, g.Key.day)).Duration,
                                       Duration = g.Sum(l => l.Duration)
                                   })
                              .OrderByDescending(t => t.DateTime)
                              .ToList();

                var requestedApp = result.Where(a => a.AppName == appName).FirstOrDefault();

                if (requestedApp != null)
                    requestedApp.IsSelected = true;

                return result;
            }
        }

        public IEnumerable<TopWindowsModel> GetLogTopWindows(int userID, string appName, IEnumerable<DateTime> days)
        {
            using (var context = new AppsEntities())
            {
                var logs = _cachedLogs == null ? _cachedLogs = context.Logs.Where(l => l.Window.Application.User.UserID == userID
                                                                                    && l.Window.Application.Name == appName)
                                                                        .Include(l => l.Window)
                                                                        .ToList()
                                                                        : _cachedLogs;

                var totalFiltered = logs.Where(l => days.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)));

                double totalDuration = totalFiltered.Sum(l => l.Duration);

                var result = (from l in totalFiltered
                              group l by l.Window.Title into grp
                              select grp).Select(g => new TopWindowsModel
                              {
                                  Title = g.Key,
                                  Usage = (g.Sum(l => l.Duration) / totalDuration),
                                  Duration = g.Sum(l => l.Duration)
                              })
                              .OrderByDescending(t => t.Duration)
                              .ToList();

                return result;
            }
        }

        public IEnumerable<DayViewModel> GetDayView(int userID, DateTime dateFrom)
        {
            using (var context = new AppsEntities())
            {
                string ignore = UsageTypes.Login.ToString();
                DateTime dateTo = dateFrom.AddDays(1);

                IEnumerable<Log> logs = null;
                IEnumerable<Usage> usages = null;

                Parallel.Invoke(() =>
                {
                    using (var newContext = new AppsEntities())
                    {
                        logs = newContext.Logs.Where(l => l.Window.Application.User.UserID == userID
                                   && l.DateCreated >= dateFrom
                                   && l.DateCreated <= dateTo)
                                 .Include(l => l.Window.Application)
                                 .ToList();
                    }

                }, () =>
                {
                    using (var newContext = new AppsEntities())
                    {
                        usages = newContext.Usages.Where(u => u.User.UserID == userID
                                         && u.UsageStart >= dateFrom
                                         && u.UsageEnd <= dateTo
                                         && u.UsageType.UType != ignore)
                                .Include(u => u.UsageType)
                                .ToList();
                    }
                }
              );

                var logModels = logs.Select(l => new DayViewModel()
                                                            {
                                                                DateCreated = l.DateCreated.ToString("HH:mm:ss"),
                                                                DateEnded = l.DateEnded.ToString("HH:mm:ss"),
                                                                Duration = l.Duration,
                                                                Name = l.Window.Application.Name,
                                                                Title = l.Window.Title
                                                            });

                var usageModels = usages.Select(u => new DayViewModel()
                                                               {
                                                                   DateCreated = u.UsageStart.ToString("HH:mm:ss"),
                                                                   DateEnded = u.UsageEnd.ToString("HH:mm:ss"),
                                                                   Duration = u.Duration.Ticks,
                                                                   Name = ((UsageTypes)Enum.Parse(typeof(UsageTypes), u.UsageType.UType)).ToExtendedString(),
                                                                   Title = "*********",
                                                                   IsRequested = true
                                                               });

                return logModels.Union(usageModels).OrderBy(d => d.DateCreated).ToList();
            }
        }

        public IEnumerable<DailyWindowSeries> GetDailyWindowSeries(int userID, string appName, IEnumerable<string> selectedWindows, IEnumerable<DateTime> days)
        {
            using (var context = new AppsEntities())
            {
                if (selectedWindows.Count() == 0)
                    return null;

                var logs = _cachedLogs == null ? _cachedLogs = context.Logs.Where(l => l.Window.Application.User.UserID == userID
                                                                                    && l.Window.Application.Name == appName)
                                                                        .Include(l => l.Window)
                                                                        .ToList()
                                                                        : _cachedLogs;

                var totalFiltered = logs.Where(l => days.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)) && selectedWindows.Contains(l.Window.Title));

                List<DailyWindowSeries> result = new List<DailyWindowSeries>();

                var projected = from l in totalFiltered
                                group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into grp
                                select grp;

                foreach (var grp in projected)
                {
                    var projected2 = grp.GroupBy(g => g.Window.Title);
                    DailyWindowSeries series = new DailyWindowSeries() { Date = new DateTime(grp.Key.year, grp.Key.month, grp.Key.day).ToShortDateString() };
                    List<DailyWindowDurationModel> modelList = new List<DailyWindowDurationModel>();
                    foreach (var grp2 in projected2)
                    {
                        DailyWindowDurationModel model = new DailyWindowDurationModel() { Title = grp2.Key, Duration = Math.Round(new TimeSpan(grp2.Sum(l => l.Duration)).TotalMinutes, 2) };
                        modelList.Add(model);
                    }
                    series.DailyWindowCollection = modelList;
                    result.Add(series);
                }

                return result;
            }
        }

        public IEnumerable<TopAppsModel> GetDayTopApps(int userID, DateTime dateFrom)
        {
            using (var context = new AppsEntities())
            {
                DateTime dateTo = dateFrom.AddDays(1);
                var logs = context.Logs.Where(l => l.Window.Application.User.UserID == userID
                                                && l.DateCreated >= dateFrom
                                                && l.DateCreated <= dateTo)
                                        .Include(l => l.Window.Application)
                                        .ToList();

                double totalDuration = (from l in logs
                                        select (double?)l.Duration).Sum() ?? 0;

                var result = (from l in logs
                              group l by l.Window.Application.Name into grp
                              select grp).ToList()
                             .Select(g => new TopAppsModel
                             {
                                 AppName = g.Key,
                                 Date = dateFrom.ToShortDateString(),
                                 Usage = (g.Sum(l => l.Duration) / totalDuration),
                                 Duration = g.Sum(l => l.Duration)
                             })
                             .OrderByDescending(t => t.Duration)
                             .ToList();

                var first = result.FirstOrDefault();
                if (first != null)
                    first.IsSelected = true;

                return result;
            }
        }

        public IEnumerable<TopWindowsModel> GetDayTopWindows(int userID, string appName, DateTime dateFrom)
        {
            if (appName == null)
                return null;
            using (var context = new AppsEntities())
            {
                var nextDay = dateFrom.AddDays(1);

                var total = context.Logs.Where(l => l.Window.Application.User.UserID == userID
                                                             && l.DateCreated >= dateFrom
                                                             && l.DateCreated <= nextDay
                                                             && l.Window.Application.Name == appName)
                                                .Include(l => l.Window)
                                                .ToList();

                double totalDuration = total.Sum(l => l.Duration);

                return total.GroupBy(l => l.Window.Title)
                                      .Select(g => new TopWindowsModel
                                      {
                                          Title = g.Key,
                                          Usage = (g.Sum(l => l.Duration) / totalDuration),
                                          Duration = g.Sum(l => l.Duration)
                                      })
                                      .OrderByDescending(t => t.Duration)
                                      .ToList();
            }
        }

        public IEnumerable<DailyUsageTypeSeries> GetDailySeries(int userID, DateTime dateFrom)
        {
            using (var context = new AppsEntities())
            {
                DateTime today = dateFrom.Date;
                DateTime nextDay = today.AddDays(1d);

                string usageLogin = UsageTypes.Login.ToString();
                string usageIdle = UsageTypes.Idle.ToString();
                string usageLocked = UsageTypes.Locked.ToString();
                string usageStopped = UsageTypes.Stopped.ToString();

                List<Usage> logins;
                List<Usage> idles = null;
                List<Usage> lockeds = null;
                List<Usage> stoppeds = null;

                logins = context.Usages.Where(u => u.User.UserID == userID
                                                && ((u.UsageStart >= today && u.UsageStart <= nextDay)
                                                        || u.IsCurrent)
                                                && u.UsageType.UType == usageLogin)
                                      .Include(u => u.UsageType)
                                      .ToList();

                var usageIDs = logins.Select(u => u.UsageID);

                Parallel.Invoke(() =>
                {
                    using (var newContext = new AppsEntities())
                    {
                        idles =
                       newContext.Usages.Where(u => u.SelfUsageID.HasValue
                               && usageIDs.Contains(u.SelfUsageID.Value)
                               && u.UsageType.UType == usageIdle)
                      .Include(u => u.UsageType)
                      .ToList();
                    }

                }, () =>
                {
                    using (var newContext = new AppsEntities())
                    {
                        lockeds = newContext.Usages.Where(u => u.SelfUsageID.HasValue
                                                                   && usageIDs.Contains(u.SelfUsageID.Value)
                                                                   && u.UsageType.UType == usageLocked)
                                                          .Include(u => u.UsageType)
                                                          .ToList();
                    }

                }, () =>
                {
                    using (var newContext = new AppsEntities())
                    {
                        stoppeds = newContext.Usages.Where(u => u.SelfUsageID.HasValue
                                                                   && usageIDs.Contains(u.SelfUsageID.Value)
                                                                   && u.UsageType.UType == usageStopped)
                                           .Include(u => u.UsageType)
                                           .ToList();
                    }

                });

                List<DailyUsageTypeSeries> collection = new List<DailyUsageTypeSeries>();

                foreach (var login in logins)
                {
                    DailyUsageTypeSeries series = new DailyUsageTypeSeries() { Time = login.UsageStart.ToString("HH:mm:ss") };
                    ObservableCollection<UsageTypeModel> observableCollection = new ObservableCollection<UsageTypeModel>();

                    long idleTime = 0;
                    long lockedTime = 0;
                    long loginTime = 0;
                    long stoppedTime = 0;

                    var currentIdles = idles.Where(u => u.SelfUsageID == login.UsageID);
                    var currentLockeds = lockeds.Where(u => u.SelfUsageID == login.UsageID);
                    var currentStopppeds = stoppeds.Where(u => u.SelfUsageID == login.UsageID);

                    if (currentIdles.Count() > 0)
                    {
                        idleTime = currentIdles.Sum(l => l.Duration.Ticks);
                        observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(idleTime).TotalHours, 2), UsageType = usageIdle });
                    }

                    if (currentLockeds.Count() > 0)
                    {
                        lockedTime = currentLockeds.Sum(l => l.Duration.Ticks);
                        observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Computer locked" });
                    }


                    if (currentStopppeds.Count() > 0)
                    {
                        stoppedTime = currentStopppeds.Sum(l => l.Duration.Ticks);
                        observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Stopped logging" });
                    }

                    loginTime = login.Duration.Ticks - lockedTime - idleTime - stoppedTime;
                    observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(loginTime).TotalHours, 2), UsageType = "Work" });


                    series.DailyUsageTypeCollection = observableCollection;

                    collection.Add(series);
                }

                if (logins.Count > 1)
                {
                    DailyUsageTypeSeries seriesTotal = new DailyUsageTypeSeries() { Time = "TOTAL" };
                    ObservableCollection<UsageTypeModel> observableCollectionTotal = new ObservableCollection<UsageTypeModel>();

                    long idleTimeTotal = 0;
                    long lockedTimeTotal = 0;
                    long loginTimeTotal = 0;
                    long stoppedTimeTotal = 0;

                    if (idles.Count > 0)
                    {
                        idleTimeTotal = idles.Sum(l => l.Duration.Ticks);
                        observableCollectionTotal.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(idleTimeTotal).TotalHours, 2), UsageType = usageIdle });
                    }

                    if (lockeds.Count > 0)
                    {
                        lockedTimeTotal = lockeds.Sum(l => l.Duration.Ticks);
                        observableCollectionTotal.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTimeTotal).TotalHours, 2), UsageType = "Computer locked" });
                    }

                    if (logins.Count > 0)
                    {
                        loginTimeTotal = logins.Sum(l => l.Duration.Ticks) - lockedTimeTotal - idleTimeTotal;
                        observableCollectionTotal.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(loginTimeTotal).TotalHours, 2), UsageType = "Work" });
                    }

                    if (stoppeds.Count > 0)
                    {
                        stoppedTimeTotal = stoppeds.Sum(l => l.Duration.Ticks);
                        observableCollectionTotal.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(stoppedTimeTotal).TotalHours, 2), UsageType = "Stopped logging" });
                    }

                    seriesTotal.DailyUsageTypeCollection = observableCollectionTotal;

                    collection.Add(seriesTotal);
                }

                return collection;
            }
        }

        public IEnumerable<MostUsedAppModel> GetMostUsedApps(int userID, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                var logs = context.Logs.Include(l => l.Window.Application)
                                    .Include(l => l.Window.Application.User)
                                    .ToList();

                var grouped = logs.Where(l => l.Window.Application.User.UserID == userID
                                            && l.DateCreated >= dateFrom
                                            && l.DateCreated <= dateTo)
                                    .GroupBy(l => l.Window.Application.Name);

                return grouped.Select(g => new MostUsedAppModel()
                                                                {
                                                                    AppName = g.Key,
                                                                    Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1)
                                                                });
            }
        }

        public IEnumerable<DailyAppModel> GetSingleMostUsedApp(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                var logs = context.Logs.Where(l => l.Window.Application.Name == appName
                                                && l.Window.Application.User.UserID == userID
                                                && l.DateCreated >= dateFrom
                                                && l.DateCreated <= dateTo)
                                                .ToList();

                var grouped = logs.GroupBy(l => new
                                            {
                                                year = l.DateCreated.Year,
                                                month = l.DateCreated.Month,
                                                day = l.DateCreated.Day
                                            })
                                    .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

                return grouped.Select(g => new DailyAppModel
                                {
                                    Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                                    Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1)
                                });
            }
        }

        public IEnumerable<KeystrokeModel> GetKeystrokes(int userID, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                var logs = context.Logs.Include(l => l.Window.Application)
                                        .Include(l => l.Window.Application.User)
                                        .ToList();

                var filtered = logs.Where(l => l.Window.Application.User.UserID == userID
                                                                    && l.DateCreated >= dateFrom
                                                                    && l.DateCreated <= dateTo
                                                                    && l.Keystrokes != null);
                return filtered
                        .GroupBy(l => l.Window.Application.Name)
                        .Select(g => new KeystrokeModel()
                        {
                            AppName = g.Key,
                            Count = g.Sum(l => l.Keystrokes.Length)
                        })
                        .OrderByDescending(k => k.Count)
                        .ToList();
            }
        }

        public IEnumerable<DailyKeystrokeModel> GetKeystrokesByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                var logs = context.Logs.Include(l => l.Window.Application)
                                        .Include(l => l.Window.Application.User)
                                        .ToList();

                var filtered = logs.Where(l => l.Window.Application.User.UserID == userID
                                             && l.DateCreated >= dateFrom
                                             && l.DateCreated <= dateTo
                                             && l.Keystrokes != null
                                             && l.Window.Application.Name == appName);

                return filtered.GroupBy(l => new
                                            {
                                                year = l.DateCreated.Year,
                                                month = l.DateCreated.Month,
                                                day = l.DateCreated.Day
                                            })
                            .Select(g => new DailyKeystrokeModel()
                            {
                                Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                                Count = g.Sum(l => l.Keystrokes.Length)
                            });
            }
        }

        public IEnumerable<DailyUsedAppsSeries> GetAppsUsageSeries(int userID, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                List<DailyUsedAppsSeries> dailyUsedAppsSeriesTemp = new List<DailyUsedAppsSeries>();

                var logs = context.Logs.Include(l => l.Window.Application)
                                    .Include(l => l.Window.Application.User)
                                    .ToList();

                var grouped = logs.Where(l => l.Window.Application.User.UserID == userID
                                              && l.DateCreated >= dateFrom
                                              && l.DateCreated <= dateTo)
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
            }
        }

        public IEnumerable<ScreenshotModel> GetScreenshots(int userID, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                var screenshots = context.Screenshots.Include(s => s.Log.Window.Application)
                                                       .Include(s => s.Log.Window.Application.User)
                                                       .ToList();

                var filtered = screenshots.Where(s => s.Log.Window.Application.User.UserID == userID
                                                    && s.Date >= dateFrom
                                                    && s.Date <= dateTo)
                                                .GroupBy(s => s.Log.Window.Application.Name)
                                                .Select(g => new ScreenshotModel() { AppName = g.Key, Count = g.Count() });

                return filtered;
            }
        }

        public IEnumerable<DailyScreenshotModel> GetScreenshotsByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                var screenshots = context.Screenshots.Include(s => s.Log.Window.Application)
                                                       .Include(s => s.Log.Window.Application.User)
                                                       .ToList();

                var filtered = screenshots.Where(s => s.Log.Window.Application.User.UserID == userID
                                                     && s.Date >= dateFrom
                                                     && s.Date <= dateTo
                                                     && s.Log.Window.Application.Name == appName
                                                     )
                                            .ToList();

                var grouped = filtered.GroupBy(s => new
                                                    {
                                                        year = s.Date.Year,
                                                        month = s.Date.Month,
                                                        day = s.Date.Day
                                                    })
                                            .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

                return grouped.Select(g => new DailyScreenshotModel() { Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(), Count = g.Count() });
            }
        }

        public IEnumerable<AllUsersModel> GetAllUsers(DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                string loginType = UsageTypes.Login.ToString();
                var logins = context.Usages.Where(u => u.UsageStart >= dateFrom
                                                     && u.UsageStart <= dateTo
                                                     && u.UsageType.UType == loginType)
                                       .Include(u => u.User)
                                       .ToList();

                return logins.GroupBy(u => u.User.Name)
                                .Select(g => new AllUsersModel
                                                            {
                                                                Username = g.Key,
                                                                LoggedInTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration.Ticks)).TotalHours, 1)
                                                            });
            }
        }

        public IEnumerable<UsageTypeSeries> GetUsageSeries(string username, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                string usageLogin = UsageTypes.Login.ToString();
                string usageIdle = UsageTypes.Idle.ToString();
                string usageLocked = UsageTypes.Locked.ToString();
                string usageStopped = UsageTypes.Stopped.ToString();

                IEnumerable<Usage> idles;
                IEnumerable<Usage> lockeds;
                IEnumerable<Usage> stoppeds;

                List<UsageTypeSeries> collection = new List<UsageTypeSeries>();

                var logins = context.Usages.Where(u => u.User.Name == username
                                                     && u.UsageStart >= dateFrom
                                                     && u.UsageStart <= dateTo
                                                     && u.UsageType.UType == usageLogin)
                                         .ToList();

                var groupedLogins = logins.GroupBy(u => new
                                                {
                                                    year = u.UsageStart.Year,
                                                    month = u.UsageStart.Month,
                                                    day = u.UsageStart.Day
                                                })
                                           .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

                Parallel.ForEach(groupedLogins.ToList(), grp =>
                {
                    var usageIDs = grp.Select(u => u.UsageID);

                    using (var newContext = new AppsEntities())
                    {
                        idles = newContext.Usages.Where(u => u.SelfUsageID.HasValue
                                                && usageIDs.Contains(u.SelfUsageID.Value)
                                                && u.UsageType.UType == usageIdle)
                                                .ToList();

                        lockeds = newContext.Usages.Where(u => u.SelfUsageID.HasValue
                                                          && usageIDs.Contains(u.SelfUsageID.Value)
                                                          && u.UsageType.UType == usageLocked)
                                                          .ToList();

                        stoppeds = newContext.Usages.Where(u => u.SelfUsageID.HasValue
                                                          && usageIDs.Contains(u.SelfUsageID.Value)
                                                          && u.UsageType.UType == usageStopped)
                                                          .ToList();
                    }

                    UsageTypeSeries series = new UsageTypeSeries() { Date = new DateTime(grp.Key.year, grp.Key.month, grp.Key.day).ToShortDateString() };

                    ObservableCollection<UsageTypeModel> observableCollection = new ObservableCollection<UsageTypeModel>();

                    long idleTime = 0;
                    long lockedTime = 0;
                    long loginTime = 0;
                    long stoppedTime = 0;

                    if (idles.Count() > 0)
                    {
                        idleTime = idles.Sum(l => l.Duration.Ticks);
                        observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(idleTime).TotalHours, 2), UsageType = usageIdle });
                    }

                    if (lockeds.Count() > 0)
                    {
                        lockedTime = lockeds.Sum(l => l.Duration.Ticks);
                        observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Computer locked" });
                    }

                    if (stoppeds.Count() > 0)
                    {
                        stoppedTime = stoppeds.Sum(l => l.Duration.Ticks);
                        observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Stopped logging" });
                    }

                    loginTime = grp.Sum(l => l.Duration.Ticks) - lockedTime - idleTime - stoppedTime;
                    observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(loginTime).TotalHours, 2), UsageType = "Work" });

                    series.DailyUsageTypeCollection = observableCollection;

                    lock (@parallelLock)
                        collection.Add(series);

                });

                return collection;
            }
        }

        public Tuple<string, string, string> GetDayInfo(int userID, DateTime dateFrom)
        {
            using (var context = new AppsEntities())
            {
                DateTime today = dateFrom.Date;
                DateTime nextDay = today.AddDays(1d);
                string loginType = UsageTypes.Login.ToString();

                var logins = context.Usages.Where(u => u.User.UserID == userID
                                                     && u.UsageStart >= today
                                                     && u.UsageStart <= nextDay
                                                     && u.UsageType.UType == loginType)
                                            .ToList();

                var loginBegin = logins.OrderBy(l => l.UsageStart).FirstOrDefault();

                var loginEnd = logins.OrderByDescending(l => l.UsageEnd).FirstOrDefault();

                var totalDuraion = new TimeSpan(logins.Sum(l => l.Duration.Ticks));

                string dayBegin = loginBegin == null ? "N/A" : loginBegin.UsageStart.ToShortTimeString();
                string dayEnd = (loginEnd == null || loginEnd.IsCurrent) ? "N/A" : loginEnd.UsageEnd.ToShortTimeString();
                string totalHours = totalDuraion.ToString(@"hh\:mm");

                return new Tuple<string, string, string>(dayBegin, dayEnd, totalHours);
            }
        }

        public void Dispose()
        {

        }
    }
}
