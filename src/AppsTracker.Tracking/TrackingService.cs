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

namespace AppsTracker.Tracking
{
    [Export(typeof(ITrackingService))]
    public sealed class TrackingService : ITrackingService
    {
        private readonly IMediator mediator;
        private readonly IRepository dataService;

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

        public Uzer SelectedUser { get; private set; }

        public string UserName { get; private set; }


        [ImportingConstructor]
        public TrackingService(IMediator mediator,
                               IRepository dataService)
        {
            this.mediator = mediator;
            this.dataService = dataService;
        }

        public void Initialize(Uzer uzer, int usageID)
        {
            Ensure.NotNull(uzer, "uzer");

            UserID = uzer.UserID;
            UserName = uzer.Name;
            SelectedUserID = UserID;
            SelectedUserName = UserName;
            DateFrom = GetFirstDate(SelectedUserID);
            UsageID = usageID;
        }


        public void ChangeUser(Uzer uzer)
        {
            Ensure.NotNull(uzer, "uzer");

            SelectedUserID = uzer.UserID;
            SelectedUserName = uzer.Name;
            SelectedUser = uzer;
            ClearDateFilter();
        }

        public void ClearDateFilter()
        {
            DateFrom = GetFirstDate(SelectedUserID);
            isDateRangeFiltered = false;
        }

        public DateTime GetFirstDate(int userID)
        {
            var usages = dataService.GetOrdered<Usage, DateTime>(u => u.UserID == userID,
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
