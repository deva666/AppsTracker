#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Diagnostics;

namespace AppsTracker.Data.Utils
{
    public class AppInfo : IAppInfo
    {
        public string ProcessName { get; private set; }
        public string ProcessVersion { get; private set; }
        public string ProcessCompany { get; private set; }
        public string ProcessDescription { get; private set; }
        public string ProcessFileName { get; private set; }
        public string ProcessRealName { get; private set; }

        private AppInfo() { }

        public static AppInfo GetAppInfo(Process process)
        {
            if (process == null)
                return null;

            AppInfo appInfo = new AppInfo();

            try
            {
                if (process.MainModule.FileVersionInfo.CompanyName != null && process.MainModule.FileVersionInfo.CompanyName.ToLower().Contains("microsoft"))
                {
                    appInfo.ProcessVersion = process.MainModule.FileVersionInfo.ProductVersion ?? "";
                    appInfo.ProcessCompany = process.MainModule.FileVersionInfo.CompanyName ?? "";
                    appInfo.ProcessFileName = process.MainModule.FileVersionInfo.FileName ?? "";
                    if (string.IsNullOrEmpty(process.MainModule.FileVersionInfo.FileDescription))
                    {
                        appInfo.ProcessName = process.MainModule.FileVersionInfo.ProductName ?? "";
                        appInfo.ProcessDescription = process.MainModule.FileVersionInfo.FileDescription ?? "";
                    }
                    else
                    {
                        appInfo.ProcessName = process.MainModule.FileVersionInfo.FileDescription ?? "";
                        appInfo.ProcessDescription = process.MainModule.FileVersionInfo.ProductName ?? "";
                    }

                    appInfo.ProcessRealName = process.ProcessName ?? "";
                }
                else
                {
                    appInfo.ProcessVersion = process.MainModule.FileVersionInfo.ProductVersion ?? "";
                    appInfo.ProcessCompany = process.MainModule.FileVersionInfo.CompanyName ?? "";
                    appInfo.ProcessFileName = process.MainModule.FileVersionInfo.FileName ?? "";
                    appInfo.ProcessName = process.MainModule.FileVersionInfo.ProductName ?? "";
                    appInfo.ProcessDescription = process.MainModule.FileVersionInfo.FileDescription ?? "";
                    appInfo.ProcessRealName = process.ProcessName ?? "";
                }
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (Exception)
            {
                try
                {
                    appInfo.ProcessVersion = "";
                    appInfo.ProcessCompany = "";
                    appInfo.ProcessFileName = "";
                    appInfo.ProcessName = process.ProcessName;
                    appInfo.ProcessDescription = "";
                    appInfo.ProcessRealName = "";
                }
                catch (Exception)
                {

                    return null;
                }
            }

            return appInfo;
        }
    }
}
