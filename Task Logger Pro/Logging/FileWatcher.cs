using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using System.Collections.ObjectModel;

namespace Task_Logger_Pro.Logging
{
    public class FileWatcher : FileSystemWatcher, IDisposable
    {
        public new bool Disposed { get; private set; }

        public FileWatcher()
            : base()
        {
        }

        public new void Dispose()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed) base.Dispose(disposing);
            Disposed = true;
        }
    }
    //    #region Fields

    //    bool _disposed;
    //    FileSystemWatcher _watcher;

    //    #endregion

    //    public FileSystemWatcher Watcher { get { return _watcher; } }

    //    public bool EnableWatcher
    //    {
    //        get { return _watcher == null ? false : _watcher.EnableRaisingEvents; }
    //        set
    //        {
    //            if (!_disposed) _watcher.EnableRaisingEvents = value;
    //        }
    //    }
    //    public bool IncludeSubdirectories
    //    {
    //        get { return _watcher.IncludeSubdirectories; }
    //        set { _watcher.IncludeSubdirectories = value; }
    //    }
    //    public string FileWatcherPath
    //    {
    //        get { return _watcher.Path; }
    //        set
    //        {
    //            try
    //            {
    //                _watcher.Path = value;
    //            }
    //            catch (System.ComponentModel.Win32Exception)
    //            {
    //                _watcher.Path = "C:\\";
    //                _watcher.EnableRaisingEvents = false;
    //            }
    //        }
    //    }



    //    #region Constructor

    //    public FileWatcher()
    //    {
    //        _watcher = new FileSystemWatcher();
    //        _watcher.IncludeSubdirectories = false;
    //        _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName;
    //        _watcher.Path = "C:\\";
    //        _watcher.EnableRaisingEvents = false;
    //        _watcher.Disposed += (s, e) => 
    //        {
    //            _disposed = true;
    //            Console.WriteLine("DISPOSED WATCHER!!!!!!!!!!!!");
    //        };
    //    }

    //    #endregion

    //    #region IDisposable Members

    //    public void Dispose()
    //    {
    //        Dispose(true);
    //        GC.SuppressFinalize(this);
    //    }

    //    private void Dispose(bool disposing)
    //    {
    //        if (!_disposed)
    //        {
    //            if (disposing)
    //            {
    //                _watcher.EnableRaisingEvents = false;
    //                _watcher.Dispose();
    //                _disposed = true;
    //            }
    //        }
    //    }

    //    #endregion
    //}


}
