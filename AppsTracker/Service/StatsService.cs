#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;

namespace AppsTracker.Service
{
    [Export(typeof(IStatsService))]
    public class StatsService : IStatsService
    {
        private IEnumerable<Log> _cachedLogs = null;

        private string _cachedLogsForApp;

        public IEnumerable<AppSummary> GetAppSummary(int userID, int appID, string appName, DateTime dateFrom, DateTime dateTo)
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
                              .Select(g => new AppSummary
                                   {
                                       AppName = g.Key.name,
                                       Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString() + " " + new DateTime(g.Key.year, g.Key.month, g.Key.day).DayOfWeek.ToString(),
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

        public IEnumerable<WindowSummary> GetWindowsSummary(int userID, string appName, IEnumerable<DateTime> days)
        {

            if (_cachedLogs == null || (_cachedLogs != null && string.Equals(_cachedLogsForApp, appName) == false))
            {
                CacheLogs(userID, appName);
            }

            var logs = _cachedLogs;

            var totalFiltered = logs.Where(l => days.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)));

            double totalDuration = totalFiltered.Sum(l => l.Duration);

            var result = (from l in totalFiltered
                          group l by l.Window.Title into grp
                          select grp).Select(g => new WindowSummary
                          {
                              Title = g.Key,
                              Usage = (g.Sum(l => l.Duration) / totalDuration),
                              Duration = g.Sum(l => l.Duration)
                          })
                          .OrderByDescending(t => t.Duration)
                          .ToList();

            return result;
        }

        private void CacheLogs(int userID, string appName)
        {
            using (var context = new AppsEntities())
            {
                _cachedLogs = context.Logs.Where(l => l.Window.Application.User.UserID == userID
                                                                && l.Window.Application.Name == appName)
                                                    .Include(l => l.Window)
                                                    .ToList();
                _cachedLogsForApp = appName;
            }
        }

        public IEnumerable<WindowDurationOverview> GetWindowDurationOverview(int userID, string appName, IEnumerable<string> selectedWindows, IEnumerable<DateTime> days)
        {

            if (selectedWindows.Count() == 0)
                return null;

            if (_cachedLogs == null || (_cachedLogs != null && _cachedLogsForApp != appName))
            {
                CacheLogs(userID, appName);
            }

            var logs = _cachedLogs;

            var totalFiltered = logs.Where(l => days.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)) && selectedWindows.Contains(l.Window.Title));

            IList<WindowDurationOverview> result = new List<WindowDurationOverview>();

            var projected = from l in totalFiltered
                            group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into grp
                            select grp;

            foreach (var grp in projected)
            {
                var projected2 = grp.GroupBy(g => g.Window.Title);
                var date = new DateTime(grp.Key.year, grp.Key.month, grp.Key.day);
                WindowDurationOverview series = new WindowDurationOverview()
                {
                    Date = date.ToShortDateString() + " " + date.DayOfWeek.ToString()
                };
                List<WindowDuration> modelList = new List<WindowDuration>();
                foreach (var grp2 in projected2)
                {
                    WindowDuration model = new WindowDuration() { Title = grp2.Key, Duration = Math.Round(new TimeSpan(grp2.Sum(l => l.Duration)).TotalMinutes, 2) };
                    modelList.Add(model);
                }
                series.DurationCollection = modelList;
                result.Add(series);
            }

            return result;
        }

        public IEnumerable<LogSummary> GetLogSummary(int userID, DateTime dateFrom)
        {
            using (var context = new AppsEntities())
            {
                string ignore = UsageTypes.Login.ToString();
                DateTime dateTo = dateFrom.AddDays(1);

                IEnumerable<Log> logs = null;
                IEnumerable<Usage> usages = null;

                Parallel.Invoke(() =>
                {
                    using (var logsContext = new AppsEntities())
                    {
                        logs = logsContext.Logs.Where(l => l.Window.Application.User.UserID == userID
                                   && l.DateCreated >= dateFrom
                                   && l.DateCreated <= dateTo)
                                 .Include(l => l.Window.Application)
                                 .ToList();
                    }

                }, () =>
                {
                    using (var usagesContext = new AppsEntities())
                    {
                        usages = usagesContext.Usages.Where(u => u.User.UserID == userID
                                         && u.UsageStart >= dateFrom
                                         && u.UsageEnd <= dateTo
                                         && u.UsageType.ToString() != ignore).ToList();
                    }
                }
              );

                var logModels = logs.Select(l => new LogSummary()
                                                            {
                                                                DateCreated = l.DateCreated.ToString("HH:mm:ss"),
                                                                DateEnded = l.DateEnded.ToString("HH:mm:ss"),
                                                                Duration = l.Duration,
                                                                Name = l.Window.Application.Name,
                                                                Title = l.Window.Title
                                                            });

                var usageModels = usages.Select(u => new LogSummary()
                                                               {
                                                                   DateCreated = u.UsageStart.ToString("HH:mm:ss"),
                                                                   DateEnded = u.UsageEnd.ToString("HH:mm:ss"),
                                                                   Duration = u.Duration.Ticks,
                                                                   Name = u.UsageType.ToExtendedString(),
                                                                   Title = "*********",
                                                                   IsRequested = true
                                                               });

                return logModels.Union(usageModels).OrderBy(d => d.DateCreated).ToList();
            }
        }

        public IEnumerable<AppSummary> GetAllAppSummaries(int userID, DateTime dateFrom)
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
                             .Select(g => new AppSummary
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

        public IEnumerable<WindowSummary> GetWindowsSummary(int userID, string appName, DateTime dateFrom)
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
                                      .Select(g => new WindowSummary
                                      {
                                          Title = g.Key,
                                          Usage = (g.Sum(l => l.Duration) / totalDuration),
                                          Duration = g.Sum(l => l.Duration)
                                      })
                                      .OrderByDescending(t => t.Duration)
                                      .ToList();
            }
        }

        public IEnumerable<UsageByTime> GetUsageSummary(int userID, DateTime dateFrom)
        {
            using (var context = new AppsEntities())
            {
                DateTime fromDay = dateFrom.Date;
                DateTime nextDay = fromDay.AddDays(1d);
                DateTime today = DateTime.Now.Date;

                List<Usage> logins;
                List<Usage> idles = null;
                List<Usage> lockeds = null;
                List<Usage> stoppeds = null;

                logins = context.Usages.Where(u => u.User.UserID == userID
                                                && ((u.UsageStart >= fromDay && u.UsageStart <= nextDay)
                                                        || (u.IsCurrent && u.UsageStart < fromDay && today >= fromDay)
                                                        || (u.IsCurrent == false && u.UsageStart <= fromDay && u.UsageEnd >= fromDay))
                                                && u.UsageType == UsageTypes.Login)
                                      .ToList();

                var usageIDs = logins.Select(u => u.UsageID);

                Parallel.Invoke(() =>
                {
                    using (var newContext = new AppsEntities())
                    {
                        idles = newContext.Usages.Where(u => u.SelfUsageID.HasValue
                                                                && usageIDs.Contains(u.SelfUsageID.Value)
                                                                && u.UsageType == UsageTypes.Idle)
                                                        .ToList();
                    }

                }, () =>
                {
                    using (var newContext = new AppsEntities())
                    {
                        lockeds = newContext.Usages.Where(u => u.SelfUsageID.HasValue
                                                                   && usageIDs.Contains(u.SelfUsageID.Value)
                                                                   && u.UsageType == UsageTypes.Locked)
                                                          .ToList();
                    }

                }, () =>
                {
                    using (var newContext = new AppsEntities())
                    {
                        stoppeds = newContext.Usages.Where(u => u.SelfUsageID.HasValue
                                                                   && usageIDs.Contains(u.SelfUsageID.Value)
                                                                   && u.UsageType == UsageTypes.Stopped)
                                           .ToList();
                    }

                });

                List<UsageByTime> collection = new List<UsageByTime>();

                foreach (var login in logins)
                {
                    UsageByTime series = new UsageByTime() { Time = login.GetDisplayedStart(fromDay).ToString("HH:mm:ss") };
                    ObservableCollection<UsageSummary> observableCollection = new ObservableCollection<UsageSummary>();

                    long idleTime = 0;
                    long lockedTime = 0;
                    long loginTime = 0;
                    long stoppedTime = 0;

                    var tempIdles = idles.Where(u => u.SelfUsageID == login.UsageID);
                    var tempLockeds = lockeds.Where(u => u.SelfUsageID == login.UsageID);
                    var tempStopppeds = stoppeds.Where(u => u.SelfUsageID == login.UsageID);

                    idleTime = tempIdles.Sum(l => l.GetDisplayedTicks(fromDay));
                    if (idleTime > 0)
                        observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(idleTime).TotalHours, 2), UsageType = "Idle" });

                    lockedTime = tempLockeds.Sum(l => l.GetDisplayedTicks(fromDay));
                    if (lockedTime > 0)
                        observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Computer locked" });

                    stoppedTime = tempStopppeds.Sum(l => l.GetDisplayedTicks(fromDay));
                    if (stoppedTime > 0)
                        observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Stopped logging" });

                    loginTime = login.GetDisplayedTicks(fromDay) - lockedTime - idleTime - stoppedTime;
                    observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(loginTime).TotalHours, 2), UsageType = "Work" });

                    series.UsageSummaryCollection = observableCollection;

                    collection.Add(series);
                }

                if (logins.Count > 1)
                {
                    UsageByTime seriesTotal = new UsageByTime() { Time = "TOTAL" };
                    ObservableCollection<UsageSummary> observableTotal = new ObservableCollection<UsageSummary>();

                    long idleTimeTotal = 0;
                    long lockedTimeTotal = 0;
                    long loginTimeTotal = 0;
                    long stoppedTimeTotal = 0;

                    idleTimeTotal = idles.Sum(l => l.GetDisplayedTicks(fromDay));
                    if (idleTimeTotal > 0)
                        observableTotal.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(idleTimeTotal).TotalHours, 2), UsageType = "Idle" });

                    lockedTimeTotal = lockeds.Sum(l => l.GetDisplayedTicks(fromDay));
                    if (lockedTimeTotal > 0)
                        observableTotal.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedTimeTotal).TotalHours, 2), UsageType = "Computer locked" });

                    stoppedTimeTotal = stoppeds.Sum(l => l.GetDisplayedTicks(fromDay));
                    if (stoppedTimeTotal > 0)
                        observableTotal.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(stoppedTimeTotal).TotalHours, 2), UsageType = "Stopped logging" });

                    loginTimeTotal = logins.Sum(l => l.GetDisplayedTicks(fromDay)) - lockedTimeTotal - idleTimeTotal;
                    observableTotal.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(loginTimeTotal).TotalHours, 2), UsageType = "Work" });

                    seriesTotal.UsageSummaryCollection = observableTotal;

                    collection.Add(seriesTotal);
                }

                return collection;
            }
        }

        public IEnumerable<AppDuration> GetAppsDuration(int userID, DateTime dateFrom, DateTime dateTo)
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

                return grouped.Select(g => new AppDuration()
                                                         {
                                                             Name = g.Key,
                                                             Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1)
                                                         });
            }
        }

        public IEnumerable<DailyAppDuration> GetAppDurationByDate(int userID, string appName, DateTime dateFrom, DateTime dateTo)
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

                return grouped.Select(g => new DailyAppDuration
                                {
                                    Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                                    Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1)
                                });
            }
        }

        public IEnumerable<AppDurationOverview> GetAppsUsageSeries(int userID, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                List<AppDurationOverview> dailyUsedAppsSeries = new List<AppDurationOverview>();

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

                List<AppDuration> dailyUsedAppsCollection;

                foreach (var app in dailyApps)
                {
                    if (app.Duration > 0)
                    {
                        if (!dailyUsedAppsSeries.Exists(d => d.Date == app.Date.ToShortDateString()))
                        {
                            dailyUsedAppsCollection = new List<AppDuration>();
                            dailyUsedAppsCollection.Add(new AppDuration() { Name = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
                            dailyUsedAppsSeries.Add(new AppDurationOverview() { Date = app.Date.ToShortDateString(), AppCollection = dailyUsedAppsCollection });
                        }
                        else
                        {
                            dailyUsedAppsSeries.First(d => d.Date == app.Date.ToShortDateString())
                                .AppCollection.Add(new AppDuration() { Name = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
                        }
                    }
                }

                foreach (var item in dailyUsedAppsSeries)
                    item.AppCollection.Sort((x, y) => x.Duration.CompareTo(y.Duration));

                return dailyUsedAppsSeries;
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

        public IEnumerable<UserLoggedTime> GetAllUsers(DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                var logins = context.Usages.Where(u => u.UsageStart >= dateFrom
                                                     && u.UsageStart <= dateTo
                                                     && u.UsageType == UsageTypes.Login)
                                       .Include(u => u.User)
                                       .ToList();

                return logins.GroupBy(u => u.User.Name)
                                .Select(g => new UserLoggedTime
                                                            {
                                                                Username = g.Key,
                                                                LoggedInTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration.Ticks)).TotalHours, 1)
                                                            });
            }
        }

        public IEnumerable<UsageOverview> GetUsageSeries(string username, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                IEnumerable<Usage> idles;
                IEnumerable<Usage> lockeds;
                IEnumerable<Usage> stoppeds;

                List<UsageOverview> collection = new List<UsageOverview>();

                var tempLogins = context.Usages.Where(u => u.User.Name == username
                                                     && u.UsageStart >= dateFrom
                                                     && u.UsageStart <= dateTo
                                                     && u.UsageType == UsageTypes.Login)
                                         .ToList();

                var logins = BreakUsagesByDay(tempLogins);

                var groupedLogins = logins.GroupBy(u => new
                                                {
                                                    year = u.UsageStart.Year,
                                                    month = u.UsageStart.Month,
                                                    day = u.UsageStart.Day
                                                })
                                           .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

                foreach (var grp in groupedLogins)
                {
                    var usageIDs = grp.Select(u => u.UsageID);

                    idles = context.Usages.Where(u => u.SelfUsageID.HasValue
                                                      && usageIDs.Contains(u.SelfUsageID.Value)
                                                      && u.UsageType == UsageTypes.Idle)
                                                      .ToList();

                    lockeds = context.Usages.Where(u => u.SelfUsageID.HasValue
                                                      && usageIDs.Contains(u.SelfUsageID.Value)
                                                      && u.UsageType == UsageTypes.Locked)
                                                      .ToList();

                    stoppeds = context.Usages.Where(u => u.SelfUsageID.HasValue
                                                      && usageIDs.Contains(u.SelfUsageID.Value)
                                                      && u.UsageType == UsageTypes.Stopped)
                                                      .ToList();

                    var day = new DateTime(grp.Key.year, grp.Key.month, grp.Key.day);

                    UsageOverview series = new UsageOverview()
                    {
                        DateInstance = day,
                        Date = day.ToShortDateString()
                    };

                    ObservableCollection<UsageSummary> observableCollection = new ObservableCollection<UsageSummary>();

                    long idleTime = 0;
                    long lockedTime = 0;
                    long loginTime = 0;
                    long stoppedTime = 0;

                    idleTime = idles.Sum(l => l.GetDisplayedTicks(day));
                    if (idleTime > 0)
                        observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(idleTime).TotalHours, 2), UsageType = "Idle" });

                    lockedTime = lockeds.Sum(l => l.GetDisplayedTicks(day));
                    if (lockedTime > 0)
                        observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Computer locked" });

                    stoppedTime = stoppeds.Sum(l => l.GetDisplayedTicks(day));
                    if (stoppedTime > 0)
                        observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Stopped logging" });

                    loginTime = grp.Sum(l => l.GetDisplayedTicks(day)) - lockedTime - idleTime - stoppedTime;
                    observableCollection.Add(new UsageSummary() { Time = Math.Round(new TimeSpan(loginTime).TotalHours, 2), UsageType = "Work" });

                    series.UsageCollection = observableCollection;
                    collection.Add(series);
                }

                return collection.OrderBy(c => c.DateInstance);
            }
        }

        public Tuple<string, string, string> GetDayInfo(int userID, DateTime dateFrom)
        {
            using (var context = new AppsEntities())
            {
                DateTime fromDay = dateFrom.Date;
                DateTime nextDay = fromDay.AddDays(1d);
                DateTime today = DateTime.Now.Date;

                var usageQuery = context.Usages.Where(u => u.User.UserID == userID
                                                     && ((u.UsageStart >= fromDay
                                                     && u.UsageStart <= nextDay)
                                                        || (u.IsCurrent && u.UsageStart < fromDay && today >= fromDay)
                                                        || (u.IsCurrent == false && u.UsageStart <= fromDay && u.UsageEnd >= fromDay))
                                                     && u.UsageType == UsageTypes.Login)
                                            .ToList();

                var durationQuery = context.Logs.Where(l => l.DateCreated >= dateFrom
                    && l.DateCreated <= nextDay).ToList();

                var logins = BreakUsagesByDay(usageQuery);

                var loginBegin = logins.Where(l => l.UsageStart >= fromDay).OrderBy(l => l.UsageStart).FirstOrDefault();

                var loginEnd = logins.Where(l => l.UsageEnd <= nextDay).OrderByDescending(l => l.UsageEnd).FirstOrDefault();

                string dayBegin = loginBegin == null ? "N/A" : loginBegin.GetDisplayedStart(fromDay).ToShortTimeString();
                string dayEnd;

                if (loginEnd == null || loginEnd.UsageEnd.Date == today)
                {
                    dayEnd = "N/A";
                }
                else if (loginEnd.IsCurrent && loginEnd.UsageEnd.Date < today)
                {
                    dayEnd = "24:00";
                }
                else
                {
                    dayEnd = loginEnd.GetDisplayedEnd(fromDay).ToShortTimeString();
                }

                var durationSpan = new TimeSpan(durationQuery.Sum(l => l.Duration));
                var duration = durationSpan.Days > 0 ? string.Format("{0:D2}:{1:D2}:{2:D2}", durationSpan.Days, durationSpan.Hours, durationSpan.Minutes) : string.Format("{0:D2}:{1:D2}", durationSpan.Hours, durationSpan.Minutes);

                return new Tuple<string, string, string>(dayBegin, dayEnd, duration);
            }
        }

        public IEnumerable<CategoryDuration> GetCategories(int userID, DateTime dateFrom)
        {
            using (var context = new AppsEntities())
            {
                DateTime dateTo = dateFrom.AddDays(1);
                List<CategoryDuration> categoryModels = new List<CategoryDuration>();

                var categories = context.AppCategories.Include(c => c.Applications)
                    .Include(c => c.Applications.Select(a => a.Windows.Select(w => w.Logs)))
                    .Where(c => c.Applications.Count > 0 &&
                           c.Applications.SelectMany(a => a.Windows).SelectMany(w => w.Logs).Where(l => l.DateCreated >= dateFrom).Any() &&
                           c.Applications.SelectMany(a => a.Windows).SelectMany(w => w.Logs).Where(l => l.DateCreated <= dateTo).Any());

                foreach (var cat in categories)
                {
                    var totalDuration = cat.Applications.SelectMany(a => a.Windows).SelectMany(w => w.Logs).Where(l => l.DateCreated >= dateFrom && l.DateCreated <= dateTo).Sum(l => l.Duration);
                    categoryModels.Add(new CategoryDuration()
                    {
                        Name = cat.Name,
                        TotalTime = Math.Round(new TimeSpan(totalDuration).TotalHours, 2)
                    });
                }

                return categoryModels;
            }
        }


        public IEnumerable<CategoryDuration> GetCategoryStats(int userId, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                List<CategoryDuration> categoryModels = new List<CategoryDuration>();

                var categories = context.AppCategories.Include(c => c.Applications)
                    .Include(c => c.Applications.Select(a => a.Windows.Select(w => w.Logs)))
                    .Where(c => c.Applications.Count > 0 &&
                           c.Applications.SelectMany(a => a.Windows).SelectMany(w => w.Logs).Where(l => l.DateCreated >= dateFrom).Any() &&
                           c.Applications.SelectMany(a => a.Windows).SelectMany(w => w.Logs).Where(l => l.DateCreated <= dateTo).Any());

                foreach (var cat in categories)
                {
                    var totalDuration = cat.Applications.SelectMany(a => a.Windows)
                        .SelectMany(w => w.Logs)
                        .Where(l => l.DateCreated >= dateFrom && l.DateCreated <= dateTo)
                        .Sum(l => l.Duration);

                    categoryModels.Add(new CategoryDuration()
                    {
                        Name = cat.Name,
                        TotalTime = Math.Round(new TimeSpan(totalDuration).TotalHours, 2)
                    });
                }

                return categoryModels;
            }
        }


        public IEnumerable<DailyCategoryDuration> GetDailyCategoryStats(int userId, string categoryName, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new AppsEntities())
            {
                var logs = context.Logs.Where(l => l.Window.Application.Categories.Any(c => c.Name == categoryName)
                                                && l.DateCreated >= dateFrom
                                                && l.DateCreated <= dateTo)
                                        .ToList();

                var grouped = logs.GroupBy(l => new
                {
                    year = l.DateCreated.Year,
                    month = l.DateCreated.Month,
                    day = l.DateCreated.Day
                });

                return grouped.Select(g => new DailyCategoryDuration()
                {
                    Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                    TotalTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 2)
                });
            }
        }


        private IList<Usage> BreakUsagesByDay(IEnumerable<Usage> usages)
        {
            List<Usage> tempUsages = new List<Usage>();

            foreach (var usage in usages)
            {
                if (usage.IsCurrent == false && usage.UsageEnd.Date == usage.UsageStart.Date)
                {
                    tempUsages.Add(usage);
                }
                else if (usage.IsCurrent == true)
                {
                    var startDaysInYear = GetDaysInYear(usage.UsageStart.Year);
                    var dayBegin = (startDaysInYear * usage.UsageStart.Year)
                                   + usage.UsageStart.DayOfYear - startDaysInYear;

                    var endDaysInYear = GetDaysInYear(DateTime.Now.Year);
                    var dayEnd = (endDaysInYear * DateTime.Now.Year)
                                + DateTime.Now.DayOfYear - endDaysInYear;

                    var span = dayEnd - dayBegin;

                    for (int i = 0; i <= span; i++)
                    {
                        Usage tempUsage = new Usage(usage);
                        tempUsage.UsageStart = usage.GetDisplayedStart(usage.UsageStart.Date.AddDays(i));
                        tempUsage.UsageEnd = i == span ? DateTime.Now
                           : usage.GetDisplayedEnd(usage.UsageEnd.Date.AddDays(i + 1));
                        tempUsages.Add(tempUsage);
                    }
                }
                else
                {
                    var startDaysInYear = GetDaysInYear(usage.UsageStart.Year);
                    var dayBegin = (startDaysInYear * usage.UsageStart.Year)
                       + usage.UsageStart.DayOfYear - startDaysInYear;

                    var endDaysInYear = GetDaysInYear(usage.UsageEnd.Year);
                    var dayEnd = (endDaysInYear * usage.UsageEnd.Year)
                       + usage.UsageEnd.DayOfYear - endDaysInYear;

                    var span = dayEnd - dayBegin;

                    for (int i = 0; i <= span; i++)
                    {
                        Usage tempUsage = new Usage(usage);
                        tempUsage.UsageStart = usage.GetDisplayedStart(usage.UsageStart.Date.AddDays(i));
                        tempUsage.UsageEnd = usage.GetDisplayedEnd(usage.UsageEnd.Date.AddDays(i));
                        tempUsages.Add(tempUsage);
                    }
                }
            }

            return tempUsages;
        }

        private int GetDaysInYear(int year)
        {
            var thisYear = new DateTime(year, 1, 1);
            var nextYear = new DateTime(year + 1, 1, 1);

            return (nextYear - thisYear).Days;
        }

        public void Dispose()
        {

        }
    }
}
