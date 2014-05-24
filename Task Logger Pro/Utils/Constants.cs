using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("AppLoggerLicenseKeys")]

namespace Task_Logger_Pro
{
    class Constants
    {
        internal const string APP_NAME = "Apps tracker";
        internal const string CMD_ARGS_AUTOSTART = "AUTOSTART";
        internal const int SECONDS_IN_MINUTE = 60;
        internal const int MILISECONDS_IN_SECOND = 1000;

    }
}
