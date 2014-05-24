using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AT;

namespace AT.DataAccess
{
    public class LogActions
    {
        public static bool AddLog(Log log)
        {
            try
            {
                using (var ctx = new AppsEntities())
                {
                    ctx.Logs.Add(log);
                    var entry = ctx.Entry(log);
                    int result = ctx.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.ErrorLogger.DumpExceptionInfo(ex);
                return false;
            }
        }

        public static List<Log> GetLogs()
        {
            try
            {
                using (var ctx = new AppsEntities())
                {
                    return (from l in ctx.Logs
                            select l).ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.ErrorLogger.DumpExceptionInfo(ex);
                return null;
            }
        }

        public static List<Log> GetLogs(int userID)
        {
            try
            {
                using (var ctx = new AppsEntities())
                {
                    return (from l in ctx.Logs
                            
                            select l).ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorLog.ErrorLogger.DumpExceptionInfo(ex);
                return null;
            }
        }

    }
}
