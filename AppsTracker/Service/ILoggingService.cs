using System;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;

namespace AppsTracker.Service
{
    public interface ILoggingService : IBaseService
    {
        bool DBSizeOperational { get; }

        int UserID { get; }

        string UserName { get; }

        int UsageID { get; }

        int SelectedUserID { get; }

        string SelectedUserName { get; }

        Uzer SelectedUser { get; }

        DateTime DateFrom { get; set; }

        DateTime DateTo { get; set; }

        event EventHandler DbSizeCritical;

        void Initialize(Uzer uzer, int usageID);

        void ChangeUser(Uzer uzer);

        void ClearDateFilter();

        decimal GetDBSize();

        Task<decimal> GetDBSizeAsync();

        Log CreateNewLog(string windowTitle, int usageID, int userID, AppsTracker.Data.Utils.IAppInfo appInfo, out bool newApp);

        Aplication GetApp(IAppInfo appInfo);

        Task SaveModifiedLogAsync(Log log);

        Task SaveNewScreenshotAsync(Screenshot screenshot);

        Usage LoginUser(int userID);

        Uzer GetUzer(string userName);

        Task SaveNewUsageAsync(UsageTypes usagetype, Usage usage);

        Task SaveModifiedUsageAsync(Usage usage);

        DateTime GetFirstDate(int userID);
    }
}
