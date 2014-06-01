using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Task_Logger_Pro.Logging
{
    public class FileWatcher : FileSystemWatcher, IDisposable
    {
        public new bool Disposed
        {
            get;
            private set;
        }

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
            if (!Disposed) 
                base.Dispose(disposing);
            Disposed = true;
        }
    }

}
