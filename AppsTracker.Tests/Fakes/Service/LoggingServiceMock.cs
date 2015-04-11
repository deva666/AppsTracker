using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    [Export(typeof(ILoggingService))]
    class LoggingServiceMock : ILoggingService
    {
        public Data.Models.Log CreateNewLog(string windowTitle, int usageID, int userID, Data.Utils.IAppInfo appInfo, out bool newApp)
        {
            newApp = false;
            return new Log(new Window(windowTitle), usageID);
        }

        public Data.Models.Aplication GetApp(Data.Utils.IAppInfo appInfo)
        {
            return new Aplication(appInfo);
        }

        public Task SaveModifiedLogAsync(Data.Models.Log log)
        {
            return Task.Delay(50);
        }

        public Task SaveNewScreenshotAsync(Data.Models.Screenshot screenshot)
        {
            return Task.Delay(50);
        }

        public Data.Models.Usage LoginUser(int userID)
        {
            return new Usage(userID);
        }

        public Data.Models.Uzer GetUzer(string userName)
        {
            return new Uzer() { Name = userName };
        }

        public Task SaveNewUsageAsync(Data.Models.UsageTypes usagetype, Data.Models.Usage usage)
        {
            return Task.Delay(50);
        }

        public Task SaveModifiedUsageAsync(Data.Models.Usage usage)
        {
            return Task.Delay(50);
        }

        public DateTime GetFirstDate(int userID)
        {
            return DateTime.Now.AddDays(-30);
        }

        public bool DBSizeOperational
        {
            get;
            set;
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

        public event EventHandler DbSizeCritical;

        public void Initialize(Uzer uzer, int usageID)
        {
            
        }

        public void ChangeUser(Uzer uzer)
        {
            
        }

        public void ClearDateFilter()
        {
            
        }

        public decimal GetDBSize()
        {
            return 1;
        }

        public Task<decimal> GetDBSizeAsync()
        {
            return Task.FromResult<decimal>(1);
        }
    }
}
