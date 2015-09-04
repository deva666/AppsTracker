using System;
using System.ComponentModel.Composition;
using System.IO;

namespace AppsTracker.Common.Logging
{
    [Export(typeof(ILogger))]
    public class FileLogger : ILogger
    {
        public void Log(System.Exception fail)
        {
            if (fail == null)
                throw new ArgumentNullException("fail");

            string errorLog = string.Format("*** ERROR on {0} ***\n\n{1}\n\n*** ERROR ***\n\n*** INNER EXCEPTION ***\n\n{2}\n\n*** INNER EXCEPTION ***\n\n*** STACK TRACE ***\n\n{3}\n\n*** STACK TRACE ***\n\n",
              DateTime.Now, fail.Message, fail.InnerException, fail.StackTrace);
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
