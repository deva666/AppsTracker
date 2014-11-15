using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Models.Proxy
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
