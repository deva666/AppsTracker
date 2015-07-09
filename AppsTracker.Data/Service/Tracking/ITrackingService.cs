using System;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;

namespace AppsTracker.Data.Service
{
    public interface ITrackingService : IBaseService
    {
        int UserID { get; }

        string UserName { get; }

        int UsageID { get; }

        int SelectedUserID { get; }

        string SelectedUserName { get; }

        Uzer SelectedUser { get; }

        DateTime DateFrom { get; set; }

        DateTime DateTo { get; set; }

        void Initialize(Uzer uzer, int usageID);

        void ChangeUser(Uzer uzer);

        void ClearDateFilter();

        Log CreateNewLog(string windowTitle, int usageID, int userID, AppInfo appInfo, out bool newApp);

        Aplication GetApp(AppInfo appInfo, int userId = default(int));

        Usage LoginUser(int userID);

        Uzer GetUzer(string userName);

        DateTime GetFirstDate(int userID);

        long GetDayDuration(Aplication app);

        long GetWeekDuration(Aplication app);
    }
}
