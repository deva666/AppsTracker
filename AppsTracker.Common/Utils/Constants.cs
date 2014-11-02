using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("AppLoggerLicenseKeys")]

namespace AppsTracker
{
    public class Constants
    {
        public const string APP_NAME = "Apps tracker";
        public const string CMD_ARGS_AUTOSTART = "AUTOSTART";
        public const int SECONDS_IN_MINUTE = 60;
        public const int MILISECONDS_IN_SECOND = 1000;
    }
}
