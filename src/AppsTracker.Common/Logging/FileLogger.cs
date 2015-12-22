using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using AppsTracker.Common.Utils;

namespace AppsTracker.Common.Logging
{
    [Export(typeof(ILogger))]
    public class FileLogger : ILogger
    {
        public void Log(Exception fail)
        {
            Ensure.NotNull(fail, "fail");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(String.Format("*** ERROR on {0} ***", DateTime.Now.ToString()));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(fail.Message);
            if (fail.InnerException != null)
            {
                stringBuilder.AppendLine(String.Format("Inner exception: {0}", fail.InnerException.Message));
            }
            stringBuilder.AppendLine("Stack trace: ");
            stringBuilder.AppendLine(fail.StackTrace);
            stringBuilder.AppendLine();

            FileStream stream = null;

            try
            {
                stream = new FileStream("ErrorLog.log", FileMode.Append, FileAccess.Write, FileShare.Read);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    stream = null;
                    writer.Write(stringBuilder.ToString());
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
