using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Proxy;

namespace AppsTracker.DAL.Service
{
    public interface IAppsService : IBaseService
    {
        Log CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp);
        Uzer InitUzer(string userName);
        Usage InitLogin();
    }
}
