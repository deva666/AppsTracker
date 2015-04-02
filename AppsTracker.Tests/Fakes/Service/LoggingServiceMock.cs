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
            get { throw new NotImplementedException(); }
        }

        public int UserID
        {
            get { throw new NotImplementedException(); }
        }

        public string UserName
        {
            get { throw new NotImplementedException(); }
        }

        public int UsageID
        {
            get { throw new NotImplementedException(); }
        }

        public int SelectedUserID
        {
            get { throw new NotImplementedException(); }
        }

        public string SelectedUserName
        {
            get { throw new NotImplementedException(); }
        }

        public Uzer SelectedUser
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime DateFrom
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime DateTo
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler DbSizeCritical;

        public void Initialize(Uzer uzer, int usageID)
        {
            throw new NotImplementedException();
        }

        public void ChangeUser(Uzer uzer)
        {
            throw new NotImplementedException();
        }

        public void ClearDateFilter()
        {
            throw new NotImplementedException();
        }

        public decimal GetDBSize()
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetDBSizeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
