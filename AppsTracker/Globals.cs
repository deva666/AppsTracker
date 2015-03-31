#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.ServiceLocation;

namespace AppsTracker
{
    public static class Globals
    {
        private static bool isDateRangeFiltered;       
        private static DateTime dateFrom;
        private static DateTime dateTo;

        public static bool DBSizeOperational { get; private set; }
        public static int UserID { get; private set; }
        public static string UserName { get; private set; }
        public static int UsageID { get; private set; }
        public static int SelectedUserID { get; private set; }
        public static string SelectedUserName { get; private set; }
        public static Uzer SelectedUser { get; private set; }

        public static DateTime DateFrom
        {
            get { return dateFrom; }
            set { dateFrom = value; }
        }

        public static DateTime DateTo
        {
            get
            {
                if (isDateRangeFiltered)
                    return dateTo;
                else
                    return DateTime.Now;
            }
            set
            {
                isDateRangeFiltered = true;
                dateTo = value;
            }
        }

        public static event EventHandler DBCleaningRequired;

        public static void Initialize(Uzer uzer, int usageID)
        {
            UserID = uzer.UserID;
            UserName = uzer.Name;
            SelectedUserID = UserID;
            SelectedUserName = UserName;
            dateFrom = GetFirstDate();
            UsageID = usageID;
        }

        public static void ClearDateFilter()
        {
            dateFrom = GetFirstDate();
            isDateRangeFiltered = false;
        }

        private static DateTime GetFirstDate()
        {
            return ServiceLocator.Instance.Resolve<ILoggingService>().GetFirstDate(SelectedUserID);
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
                FileInfo file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "appsdb.sdf"));
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
                FileLogger.Instance.Log(ex);
                return -1;
            }
        }

        public static Task<decimal> GetDBSizeAsync()
        {
            return Task<decimal>.Run(new Func<decimal>(GetDBSize));
        }

    }
}
