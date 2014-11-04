using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.ChartModels;

namespace AppsTracker.DAL.Service
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
        IEnumerable<KeystrokeModel> GetKeystrokesByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo);
        IEnumerable<DailyUsedAppsSeries> GetAppsUsageSeries(int userID, DateTime dateFrom, DateTime dateTo);
        IEnumerable<ScreenshotModel> GetScreenshots(int userID, DateTime dateFrom, DateTime dateTo);
        IEnumerable<DailyScreenshotModel> GetScreenshotsByApp(int userID, string appName, DateTime dateFrom, DateTime dateTo);
        IEnumerable<AllUsersModel> GetAllUsers(DateTime dateFrom, DateTime dateTo);
        IEnumerable<UsageTypeSeries> GetUsageSeries(int userID, DateTime dateFrom, DateTime dateTo);
        Tuple<string, string, string> GetDayInfo(int userID, DateTime dateFrom);
    }
}
