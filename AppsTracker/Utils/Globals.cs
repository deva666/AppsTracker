#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;

namespace AppsTracker
{
    public static class Globals
    {
        private static bool _isLastDateFiltered;

        private static DateTime _date1;
        private static DateTime _date2;

        public static bool DBSizeOperational { get; private set; }
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

        public static event EventHandler DBCleaningRequired;

        public static void Initialize(Uzer uzer, int usageID)
        {
            UserID = uzer.UserID;
            UserName = uzer.Name;
            SelectedUserID = UserID;
            SelectedUserName = UserName;
            _date1 = GetFirstDate();
            UsageID = usageID;
        }

        public static void ClearDateFilter()
        {
            _date1 = GetFirstDate();
            _isLastDateFiltered = false;
        }

        private static DateTime GetFirstDate()
        {
            using (var context = new AppsEntities())
            {
                return context.Usages.Count(u => u.UserID == SelectedUserID) == 0 ? DateTime.Now.Date
                    : context.Usages.Where(u => u.UserID == SelectedUserID).Select(u => u.UsageStart).Min();
            }
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
                decimal size = Math.Round((decimal)file.Length / 1048576, 2);
                if (size >= 3900m)
                {
                    DBSizeOperational = false;
                    DBCleaningRequired.InvokeSafely(typeof(Globals), EventArgs.Empty);
                }
                else
                    DBSizeOperational = true;
                return size;
            }
            catch (Exception ex)
            {
                Exceptions.FileLogger.Log(ex);
                return -1;
            }
        }

        public static Task<decimal> GetDBSizeAsync()
        {
            return Task<decimal>.Run(new Func<decimal>(GetDBSize));
        }

    }
}
