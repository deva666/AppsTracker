using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Users;

namespace AppsTracker.Domain.Util
{
    public static class Extensions
    {
        public static string ToExtendedString(this UsageTypes type)
        {
            switch (type)
            {
                case UsageTypes.Login:
                    return "USER LOGGED IN";
                case UsageTypes.Idle:
                    return "USER IDLE";
                case UsageTypes.Locked:
                    return "SYSTEM LOCKED";
                case UsageTypes.Stopped:
                    return "LOGGING STOPPED";
                default:
                    return string.Empty;
            }
        }

        public static UserModel ToModel(this Uzer uzer)
        {
            return new UserModel(uzer);
        }
    }
}
