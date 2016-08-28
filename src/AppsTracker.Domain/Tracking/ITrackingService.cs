using System;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Users;

namespace AppsTracker.Domain.Tracking
{
    public interface ITrackingService
    {
        int UserID { get; }

        string UserName { get; }

        int UsageID { get; }

        int SelectedUserID { get; }

        string SelectedUserName { get; }

        UserModel SelectedUser { get; }

        DateTime DateFrom { get; set; }

        DateTime DateTo { get; set; }

        void Initialize(UserModel user, int usageID);

        void ChangeUser(UserModel user);

        void ClearDateFilter();

        DateTime GetFirstDate(int userID);
    }
}
