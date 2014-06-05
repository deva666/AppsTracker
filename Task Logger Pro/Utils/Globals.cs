using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppsTracker.Models.EntityModels;
using AppsTracker.DAL;

namespace Task_Logger_Pro
{
    public static class Globals
    {
        public static int UsageTypeLoginID;
        public static int UsageTypeIdleID;
        public static int UsageTypeLockedID;

        private static bool _isLastDateFiltered;

        private static DateTime _date1;
        private static DateTime _date2;

        public static int UserID { get; private set; }
        public static string UserName { get; private set; }
        public static int UsageID { get; private set; }
        public static int SelectedUserID { get; private set; }
        public static string SelectedUserName { get; private set; }
        public static Uzer SelectedUser { get; private set; }
        public static DateTime Date1
        {
            get
            {
                return _date1;
            }
            set
            {
                _date1 = value;
            }
        }

        public static DateTime Date2
        {
            get
            {
                if (_isLastDateFiltered)
                    return _date2;
                else
                    return DateTime.Now;
            }
            set
            {
                _isLastDateFiltered = true;
                _date2 = value;
            }
        }

        public static void Initialize(Uzer uzer, int usageID, AppsEntities context)
        {
            UserID = uzer.UserID;
            UserName = uzer.Name;
            SelectedUserID = UserID;
            SelectedUserName = UserName;
            _date1 = GetFirstDate(context);
            UsageID = usageID;
        }

        public static void ClearDateFilter()
        {
            using (var context = new AppsEntities())
            {
                _date1 = GetFirstDate(context);
            }
            _isLastDateFiltered = false;
        }

        private static DateTime GetFirstDate(AppsEntities context)
        {
            return context.Usages.Count() == 0 ? DateTime.Now.Date : (from u in context.Users.AsNoTracking()
                                                                      join l in context.Usages.AsNoTracking() on u.UserID equals l.UserID
                                                                      where u.UserID == SelectedUserID
                                                                      select l.UsageStart).Min();
        }

        public static void ChangeUser(Uzer uzer)
        {
            if (uzer == null)
                throw new ArgumentNullException("User");
            SelectedUserID = uzer.UserID;
            SelectedUserName = uzer.Name;
            SelectedUser = uzer;
            ClearDateFilter();
        }

        public static decimal GetDBSize()
        {
            try
            {
                FileInfo file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "apps.sdf"));
                return Math.Round((decimal)file.Length / 1048576, 2);
            }
            catch (Exception ex)
            {
                Exceptions.Logger.DumpExceptionInfo(ex);
                return -1;
            }
        }

    }
}
