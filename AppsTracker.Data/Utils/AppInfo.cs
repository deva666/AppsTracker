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
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Company { get; private set; }
        public string Description { get; private set; }
        public string FileName { get; private set; }
        public string FullName { get; private set; }

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
                    appInfo.Version = process.MainModule.FileVersionInfo.ProductVersion ?? "";
                    appInfo.Company = process.MainModule.FileVersionInfo.CompanyName ?? "";
                    appInfo.FileName = process.MainModule.FileVersionInfo.FileName ?? "";
                    if (string.IsNullOrEmpty(process.MainModule.FileVersionInfo.FileDescription))
                    {
                        appInfo.Name = process.MainModule.FileVersionInfo.ProductName ?? "";
                        appInfo.Description = process.MainModule.FileVersionInfo.FileDescription ?? "";
                    }
                    else
                    {
                        appInfo.Name = process.MainModule.FileVersionInfo.FileDescription ?? "";
                        appInfo.Description = process.MainModule.FileVersionInfo.ProductName ?? "";
                    }

                    appInfo.FullName = process.ProcessName ?? "";
                }
                else
                {
                    appInfo.Version = process.MainModule.FileVersionInfo.ProductVersion ?? "";
                    appInfo.Company = process.MainModule.FileVersionInfo.CompanyName ?? "";
                    appInfo.FileName = process.MainModule.FileVersionInfo.FileName ?? "";
                    appInfo.Name = process.MainModule.FileVersionInfo.ProductName ?? "";
                    appInfo.Description = process.MainModule.FileVersionInfo.FileDescription ?? "";
                    appInfo.FullName = process.ProcessName ?? "";
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
                    appInfo.Version = "";
                    appInfo.Company = "";
                    appInfo.FileName = "";
                    appInfo.Name = process.ProcessName;
                    appInfo.Description = "";
                    appInfo.FullName = "";
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
