using System;
using System.ComponentModel.Composition;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    [Export(typeof(ITrackingService))]
    class TrackingServiceMock : ITrackingService
    {
        public Data.Models.Log CreateNewLog(string windowTitle, int usageID, int userID, Data.Utils.IAppInfo appInfo, out bool newApp)
        {
            newApp = false;
            return new Log(new Window(windowTitle), usageID);
        }

        public Data.Models.Aplication GetApp(Data.Utils.IAppInfo appInfo, int userId = default(int))
        {
            return new Aplication(appInfo);
        }

        public Data.Models.Usage LoginUser(int userID)
        {
            return new Usage(userID);
        }

        public Data.Models.Uzer GetUzer(string userName)
        {
            return new Uzer() { Name = userName };
        }

        public DateTime GetFirstDate(int userID)
        {
            return DateTime.Now.AddDays(-30);
        }

        public int UserID
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public int UsageID
        {
            get;
            set;
        }

        public int SelectedUserID
        {
            get;
            set;
        }

        public string SelectedUserName
        {
            get;
            set;
        }

        public Uzer SelectedUser
        {
            get;
            set;
        }

        public DateTime DateFrom
        {
            get;
            set;
        }

        public DateTime DateTo
        {
            get;
            set;
        }


        public void Initialize(Uzer uzer, int usageID)
        {

        }

        public void ChangeUser(Uzer uzer)
        {

        }

        public void ClearDateFilter()
        {

        }



        public long GetDayDuration(Aplication app)
        {
            throw new NotImplementedException();
        }

        public long GetWeekDuration(Aplication app)
        {
            throw new NotImplementedException();
        }
    }
}
