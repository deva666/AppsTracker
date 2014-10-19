using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Task_Logger_Pro.Exceptions
{
    public static class Logger
    {
        public static void DumpExceptionInfo( System.Exception ex )
        {
            if (ex == null)
                throw new ArgumentNullException();
            string errorLog = string.Format( "*** ERROR on {0} ***\n\n{1}\n\n*** ERROR ***\n\n*** INNER EXCEPTION ***\n\n{2}\n\n*** INNER EXCEPTION ***\n\n*** STACK TRACE ***\n\n{3}\n\n*** STACK TRACE ***\n\n",
              DateTime.Now, ex.Message, ex.InnerException, ex.StackTrace );
            FileStream stream = null;
            try
            {
                stream = new FileStream( "ErrorLog.log", FileMode.Append, FileAccess.Write, FileShare.Read );
                using ( StreamWriter writer = new StreamWriter( stream ) )
                {
                    stream = null;
                    writer.Write( errorLog );
                }
            }
            finally
            {
                if ( stream != null ) 
                    stream.Dispose();
            }
        }

        public static void DumpDebug(string message)
        {
//#if DEBUG
            if (message == null)
                throw new ArgumentNullException();
            FileStream stream = null;
            try
            {
                stream = new FileStream("DebugLog.log", FileMode.Append, FileAccess.Write, FileShare.Read);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    stream = null;
                    writer.WriteLine("DEBUG ENTRY ON " + DateTime.Now.ToString());
                    writer.WriteLine(message);
                    writer.WriteLine();
                }
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            } 
//#endif
        }
    }
}
