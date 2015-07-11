using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;

namespace AppsTracker.Tracking.Windows
{
    internal sealed class LogProcessor
    {
        private LogInfo activeLogInfo;
        private Log activeLog;

        public void NewLogInfo(LogInfo logInfo)
        {
            activeLogInfo = logInfo;
            //await create log
            //if loginfo != activeLoginfo
            //then save immediately returned log
            //and active log is null
            //else activeLog is returned log
        }

        private void SaveLog()
        {

        }
    }
}
