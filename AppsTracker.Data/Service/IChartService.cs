#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;

using AppsTracker.Data.Models;

namespace AppsTracker.Data.Service
{
    public interface IChartService : IBaseService, IDisposable
    {
        IEnumerable<AppSummary> GetAppSummary(int userID, int appID, string appName, DateTime dateFrom, DateTime dateTo);
        IEnumerable<WindowSummary> GetWindowsSummary(int userID, string appName, IEnumerable<DateTime> dates);
        IEnumerable<WindowDurationOverview> GetWindowDurationOverview(int userID, string appName, IEnumerable<string> selectedWindows, IEnumerable<DateTime> days);
        IEnumerable<LogSummary> GetLogSummary(int userID, DateTime dateFrom);
        IEnumerable<AppSummary> GetAllAppSummaries(int userID, DateTime dateFrom);
        IEnumerable<WindowSummary> GetWindowsSummary(int userID, string appName, DateTime dateFrom);
        IEnumerable<UsageByTime> GetUsageSummary(int userID, DateTime dateFrom);
        IEnumerable<MostUsedAppModel> GetMostUsedApps(int userID, DateTime dateFrom, DateTime dateTo);
        IEnumerable<DailyAppModel> GetSingleMostUsedApp(int userID, string appName, DateTime dateFrom, DateTime dateTo);
        IEnumerable<KeystrokeModel> GetKeystrokes(int userID, DateTime dateFrom, DateTime dateTo);
        IEnumerable<DailyKeystrokeModel> GetKeystrokesByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo);
        IEnumerable<DailyUsedAppsSeries> GetAppsUsageSeries(int userID, DateTime dateFrom, DateTime dateTo);
        IEnumerable<ScreenshotModel> GetScreenshots(int userID, DateTime dateFrom, DateTime dateTo);
        IEnumerable<DailyScreenshotModel> GetScreenshotsByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo);
        IEnumerable<AllUsersModel> GetAllUsers(DateTime dateFrom, DateTime dateTo);
        IEnumerable<UsageTypeSeries> GetUsageSeries(string username, DateTime dateFrom, DateTime dateTo);
        Tuple<string, string, string> GetDayInfo(int userID, DateTime dateFrom);
        IEnumerable<CategoryModel> GetCategories(int userID, DateTime dateFrom);
    }
}
