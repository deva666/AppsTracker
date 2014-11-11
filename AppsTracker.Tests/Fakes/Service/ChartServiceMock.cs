using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.DAL.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    public class ChartServiceMock : IChartService
    {
        public IEnumerable<Models.ChartModels.TopAppsModel> GetLogTopApps(int userID, int appID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.TopWindowsModel> GetLogTopWindows(int userID, string appName, IEnumerable<DateTime> dates)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.DailyWindowSeries> GetDailyWindowSeries(int userID, string appName, IEnumerable<string> selectedWindows, IEnumerable<DateTime> days)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.DayViewModel> GetDayView(int userID, DateTime dateFrom)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.TopAppsModel> GetDayTopApps(int userID, DateTime dateFrom)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.TopWindowsModel> GetDayTopWindows(int userID, string appName, DateTime dateFrom)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.DailyUsageTypeSeries> GetDailySeries(int userID, DateTime dateFrom)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.MostUsedAppModel> GetMostUsedApps(int userID, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.DailyAppModel> GetSingleMostUsedApp(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.KeystrokeModel> GetKeystrokes(int userID, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.DailyKeystrokeModel> GetKeystrokesByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.DailyUsedAppsSeries> GetAppsUsageSeries(int userID, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.ScreenshotModel> GetScreenshots(int userID, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.DailyScreenshotModel> GetScreenshotsByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.AllUsersModel> GetAllUsers(DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Models.ChartModels.UsageTypeSeries> GetUsageSeries(string username, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public Tuple<string, string, string> GetDayInfo(int userID, DateTime dateFrom)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
