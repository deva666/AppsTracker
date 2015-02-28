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
        IEnumerable<TopAppsModel> GetLogTopApps(int userID, int appID, string appName, DateTime dateFrom, DateTime dateTo);
        IEnumerable<TopWindowsModel> GetLogTopWindows(int userID, string appName, IEnumerable<DateTime> dates);
        IEnumerable<DailyWindowSeries> GetDailyWindowSeries(int userID, string appName, IEnumerable<string> selectedWindows, IEnumerable<DateTime> days);
        IEnumerable<DayViewModel> GetDayView(int userID, DateTime dateFrom);
        IEnumerable<TopAppsModel> GetDayTopApps(int userID, DateTime dateFrom);
        IEnumerable<TopWindowsModel> GetDayTopWindows(int userID, string appName, DateTime dateFrom);
        IEnumerable<DailyUsageTypeSeries> GetDailySeries(int userID, DateTime dateFrom);
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
    }
}
