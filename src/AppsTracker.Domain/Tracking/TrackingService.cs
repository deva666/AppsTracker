#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using AppsTracker.Common.Communication;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Users;

namespace AppsTracker.Domain.Tracking
{
    [Export(typeof(ITrackingService))]
    public sealed class TrackingService : ITrackingService
    {
        private readonly Mediator mediator;
        private readonly IRepository repository;

        private volatile bool isDateRangeFiltered;

        private int userID;

        public int UserID
        {
            get { return userID; }
            private set { Interlocked.Exchange(ref userID, value); }
        }


        private int usageID;

        public int UsageID
        {
            get { return usageID; }
            private set { Interlocked.Exchange(ref usageID, value); }
        }

        private int selectedUserID;

        public int SelectedUserID
        {
            get { return selectedUserID; }
            private set { Interlocked.Exchange(ref selectedUserID, value); }
        }


        private long dateFromTicks;

        public DateTime DateFrom
        {
            get { return new DateTime(Interlocked.Read(ref dateFromTicks)); }
            set { Interlocked.Exchange(ref dateFromTicks, value.Ticks); }
        }


        private long dateToTicks;

        public DateTime DateTo
        {
            get
            {
                if (isDateRangeFiltered)
                    return new DateTime(Interlocked.Read(ref dateToTicks));
                else
                    return DateTime.Now;
            }
            set
            {
                isDateRangeFiltered = true;
                Interlocked.Exchange(ref dateToTicks, value.Ticks);
            }
        }

        public string SelectedUserName { get; private set; }

        public UserModel SelectedUser { get; private set; }

        public string UserName { get; private set; }


        [ImportingConstructor]
        public TrackingService(Mediator mediator,
                               IRepository repository)
        {
            this.mediator = mediator;
            this.repository = repository;
        }

        public void Initialize(UserModel user, int usageID)
        {
            Ensure.NotNull(user, "uzer");

            UserID = user.UserID;
            UserName = user.Name;
            SelectedUserID = UserID;
            SelectedUserName = UserName;
            DateFrom = GetFirstDate(SelectedUserID);
            UsageID = usageID;
        }


        public void ChangeUser(UserModel user)
        {
            Ensure.NotNull(user, "uzer");

            SelectedUserID = user.UserID;
            SelectedUserName = user.Name;
            SelectedUser = user;
            ClearDateFilter();
        }

        public void ClearDateFilter()
        {
            DateFrom = GetFirstDate(SelectedUserID);
            isDateRangeFiltered = false;
        }

        public DateTime GetFirstDate(int userID)
        {
            var usages = repository.GetOrdered<Usage, DateTime>(u => u.UserID == userID,
                                                                 u => u.UsageStart,
                                                                 1);
            if (usages.Count() == 0)
            {
                return DateTime.Now.Date;
            }
            else
            {
                return usages.First().UsageStart;
            }
        }
    }
}
