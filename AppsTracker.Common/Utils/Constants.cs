#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Runtime.CompilerServices;

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
