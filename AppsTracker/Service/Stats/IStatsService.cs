#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;

using AppsTracker.Data.Models;

namespace AppsTracker.Service
{
    public interface IStatsService : IBaseService, IDisposable
    {
        IEnumerable<AppSummary> GetAppSummary(int userID, int appID, string appName, DateTime dateFrom, DateTime dateTo);
        
        IEnumerable<WindowSummary> GetWindowsSummary(int userID, string appName, IEnumerable<DateTime> dates);
        
        IEnumerable<WindowDurationOverview> GetWindowDurationOverview(int userID, string appName, IEnumerable<string> selectedWindows, IEnumerable<DateTime> days);
        
        IEnumerable<LogSummary> GetLogSummary(int userID, DateTime dateFrom);
        
        IEnumerable<AppSummary> GetAllAppSummaries(int userID, DateTime dateFrom);
        
        IEnumerable<WindowSummary> GetWindowsSummary(int userID, string appName, DateTime dateFrom);
        
        IEnumerable<UsageByTime> GetUsageSummary(int userID, DateTime dateFrom);
        
        IEnumerable<AppDuration> GetAppsDuration(int userID, DateTime dateFrom, DateTime dateTo);
        
        IEnumerable<DailyAppDuration> GetAppDurationByDate(int userID, string appName, DateTime dateFrom, DateTime dateTo);
        
        IEnumerable<AppDurationOverview> GetAppsUsageSeries(int userID, DateTime dateFrom, DateTime dateTo);
        
        IEnumerable<ScreenshotModel> GetScreenshots(int userID, DateTime dateFrom, DateTime dateTo);
        
        IEnumerable<DailyScreenshotModel> GetScreenshotsByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo);
        
        IEnumerable<UserLoggedTime> GetAllUsers(DateTime dateFrom, DateTime dateTo);
        
        IEnumerable<UsageOverview> GetUsageSeries(string username, DateTime dateFrom, DateTime dateTo);
        
        Tuple<string, string, string> GetDayInfo(int userID, DateTime dateFrom);
        
        IEnumerable<CategoryDuration> GetCategories(int userID, DateTime dateFrom);

        IEnumerable<CategoryDuration> GetCategoryStats(int userId, DateTime dateFrom, DateTime dateTo);

        IEnumerable<DailyCategoryDuration> GetDailyCategoryStats(int userId, string categoryName, DateTime dateFrom, DateTime dateTo);
    }
}
