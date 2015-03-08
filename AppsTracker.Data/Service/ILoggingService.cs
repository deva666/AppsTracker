using System;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;
namespace AppsTracker.Data.Service
{
    public interface ILoggingService : IBaseService
    {
        Log CreateNewLog(string windowTitle, int usageID, int userID, AppsTracker.Data.Utils.IAppInfo appInfo, out bool newApp);
        Aplication GetApp(IAppInfo appInfo);
        Task SaveModifiedLogAsync(Log log);
        Task SaveNewScreenshotAsync(Screenshot screenshot);
        Usage LoginUser(int userID);
        Uzer GetUzer(string userName);
        Task SaveNewUsageAsync(UsageTypes usagetype, Usage usage);
        Task SaveModifiedUsageAsync(Usage usage);
    }
}
