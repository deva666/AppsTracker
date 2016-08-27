using System;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking
{
    public interface ITrackingService
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

        DateTime GetFirstDate(int userID);
    }
}
