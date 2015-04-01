#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using AppsTracker.Data.Models;
using AppsTracker.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    [Export(typeof(IStatsService))]
    public class StatsServiceMock : IStatsService
    {
        public IEnumerable<AppSummary> GetAppSummary(int userID, int appID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<AppSummary>();
        }

        public IEnumerable<WindowSummary> GetWindowsSummary(int userID, string appName, IEnumerable<DateTime> dates)
        {
            System.Threading.Thread.Sleep(500);
            return new List<WindowSummary>();
        }

        public IEnumerable<WindowDurationOverview> GetWindowDurationOverview(int userID, string appName, IEnumerable<string> selectedWindows, IEnumerable<DateTime> days)
        {
            System.Threading.Thread.Sleep(500);
            return new List<WindowDurationOverview>();
        }

        public IEnumerable<LogSummary> GetLogSummary(int userID, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new List<LogSummary>();
        }

        public IEnumerable<AppSummary> GetAllAppSummaries(int userID, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new List<AppSummary>();
        }

        public IEnumerable<WindowSummary> GetWindowsSummary(int userID, string appName, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new List<WindowSummary>();
        }

        public IEnumerable<UsageByTime> GetUsageSummary(int userId , DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new List<UsageByTime>();
        }

        public IEnumerable<AppDuration> GetAppsDuration(int userID, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<AppDuration>();
        }

        public IEnumerable<DailyAppDuration> GetAppDurationByDate(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<DailyAppDuration>();
        }

        public IEnumerable<AppDurationOverview> GetAppsUsageSeries(int userID, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<AppDurationOverview>();
        }

        public IEnumerable<ScreenshotModel> GetScreenshots(int userID, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<ScreenshotModel>();
        }

        public IEnumerable<DailyScreenshotModel> GetScreenshotsByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<DailyScreenshotModel>();
        }

        public IEnumerable<UserLoggedTime> GetAllUsers(DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<UserLoggedTime>();
        }

        public IEnumerable<UsageOverview> GetUsageSeries(string username, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<UsageOverview>();
        }

        public Tuple<string, string, string> GetDayInfo(int userID, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new Tuple<string, string, string>("", "", "");
        }

        public IEnumerable<CategoryDuration> GetCategories(int userID, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new List<CategoryDuration>();
        }

        public void Dispose()
        {
        }


        public IEnumerable<CategoryDuration> GetCategoryStats(int userId, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<CategoryDuration>();
        }

        public IEnumerable<DailyCategoryDuration> GetDailyCategoryStats(int userId, string categoryName, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<DailyCategoryDuration>();
        }
    }
}
