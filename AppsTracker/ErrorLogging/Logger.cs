using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel.Composition;

namespace AppsTracker
{
    [Export(typeof(ILogger))]
    public class FileLogger : ILogger
    {
        private static readonly Lazy<FileLogger> instance = new Lazy<FileLogger>(() => new FileLogger());
        public static FileLogger Instance { get { return instance.Value; } }
     
        private FileLogger()
        {
        }

        public void Log(System.Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException();
            string errorLog = string.Format("*** ERROR on {0} ***\n\n{1}\n\n*** ERROR ***\n\n*** INNER EXCEPTION ***\n\n{2}\n\n*** INNER EXCEPTION ***\n\n*** STACK TRACE ***\n\n{3}\n\n*** STACK TRACE ***\n\n",
              DateTime.Now, ex.Message, ex.InnerException, ex.StackTrace);
            FileStream stream = null;
            try
            {
                stream = new FileStream("ErrorLog.log", FileMode.Append, FileAccess.Write, FileShare.Read);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    stream = null;
                    writer.Write(errorLog);
                }
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }
        }
    }
}
