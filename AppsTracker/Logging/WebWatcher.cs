using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Fiddler;
using System.Collections.ObjectModel;

namespace Task_Logger_Pro.Logging
{
    public class WebWatcher : IDisposable
    {
        //public event EventHandler<WebWatcherEventArgs> NewWebLog;
        //public WebWatcher()
        //{
        //    FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.Default);
        //    FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
        //    FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
        //}

        //void FiddlerApplication_BeforeRequest(Session oSession)
        //{
        //    //Console.WriteLine(oSession.hostname);
        //    //Console.WriteLine(oSession.url);
        //}

        public void StopWebWatcher()
        {
            Console.WriteLine("Fiddler Shutdown!");
            //FiddlerApplication.Shutdown();
        }

        //void FiddlerApplication_AfterSessionComplete(Session oSession)
        //{
        //    if (NewWebLog != null) NewWebLog(this, new WebWatcherEventArgs(new WebLog(oSession.hostname)));
        //    //Console.WriteLine(oSession.host);
        //    // Console.WriteLine(oSession.hostname);
        //    //Console.WriteLine(oSession.WriteToStream);
        //    //Console.WriteLine(oSession.url);
        //}
        ~WebWatcher()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            StopWebWatcher();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class WebWatcherEventArgs : EventArgs
    {
        WebLog _webLog;
        public WebLog WebLog { get { return _webLog; } }

        public WebWatcherEventArgs(WebLog webLog)
        {
            _webLog = webLog;
        }
    }

    [Serializable]
    public class WebLog
    {
        private string _url;
        private DateTime _date;

        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public WebLog()
        {
            Date = DateTime.Now;
        }

        public WebLog(string url)
            : this()
        {
            Url = url;
        }

    }

    [Serializable]
    public class WebLogCollection : ObservableCollection<WebLog>
    {

    }
}
