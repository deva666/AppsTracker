#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;

using AppsTracker.DAL.Service;
using AppsTracker.Models.ChartModels;

namespace AppsTracker.Tests.Fakes.Service
{
    public class ChartServiceMock : IChartService
    {
        public IEnumerable<TopAppsModel> GetLogTopApps(int userID, int appID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<TopAppsModel>();
        }

        public IEnumerable<TopWindowsModel> GetLogTopWindows(int userID, string appName, IEnumerable<DateTime> dates)
        {
            System.Threading.Thread.Sleep(500);
            return new List<TopWindowsModel>();
        }

        public IEnumerable<DailyWindowSeries> GetDailyWindowSeries(int userID, string appName, IEnumerable<string> selectedWindows, IEnumerable<DateTime> days)
        {
            System.Threading.Thread.Sleep(500);
            return new List<DailyWindowSeries>();
        }

        public IEnumerable<DayViewModel> GetDayView(int userID, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new List<DayViewModel>();
        }

        public IEnumerable<TopAppsModel> GetDayTopApps(int userID, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new List<TopAppsModel>();
        }

        public IEnumerable<TopWindowsModel> GetDayTopWindows(int userID, string appName, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new List<TopWindowsModel>();
        }

        public IEnumerable<DailyUsageTypeSeries> GetDailySeries(int userID, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new List<DailyUsageTypeSeries>();
        }

        public IEnumerable<MostUsedAppModel> GetMostUsedApps(int userID, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<MostUsedAppModel>();
        }

        public IEnumerable<DailyAppModel> GetSingleMostUsedApp(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<DailyAppModel>();
        }

        public IEnumerable<KeystrokeModel> GetKeystrokes(int userID, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<KeystrokeModel>();
        }

        public IEnumerable<DailyKeystrokeModel> GetKeystrokesByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<DailyKeystrokeModel>();
        }

        public IEnumerable<DailyUsedAppsSeries> GetAppsUsageSeries(int userID, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<DailyUsedAppsSeries>();
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

        public IEnumerable<AllUsersModel> GetAllUsers(DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<AllUsersModel>();
        }

        public IEnumerable<UsageTypeSeries> GetUsageSeries(string username, DateTime dateFrom, DateTime dateTo)
        {
            System.Threading.Thread.Sleep(500);
            return new List<UsageTypeSeries>();
        }

        public Tuple<string, string, string> GetDayInfo(int userID, DateTime dateFrom)
        {
            System.Threading.Thread.Sleep(500);
            return new Tuple<string, string, string>("", "", "");
        }

        public void Dispose()
        {
        }
    }
}
