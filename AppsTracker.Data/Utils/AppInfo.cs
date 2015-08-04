#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Diagnostics;
using AppsTracker.Common.Utils;

namespace AppsTracker.Data.Utils
{
    public struct AppInfo
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Company { get; private set; }
        public string Description { get; private set; }
        public string FileName { get; private set; }
        public string FullName { get; private set; }

        private static AppInfo emptyAppInfo = new AppInfo()
        {
            Name = string.Empty,
            Company = string.Empty,
            Version = string.Empty,
            FileName = string.Empty,
            Description = string.Empty,
            FullName = string.Empty
        };

        public static AppInfo Empty { get { return emptyAppInfo; } }

        private static Process GetProcessFromHandle(IntPtr hWnd)
        {
            uint processID = 0;
            if (hWnd != IntPtr.Zero)
            {
                try
                {
                    WinAPI.GetWindowThreadProcessId(hWnd, out processID);
                    if (processID != 0)
                        return System.Diagnostics.Process.GetProcessById(checked((int)processID));
                }
                catch 
                {
                    return null;
                }
            }
            return null;
        }


        public static AppInfo Create(IntPtr hWnd)
        {
            var process = GetProcessFromHandle(hWnd);

            if (process == null)
                return emptyAppInfo;

            var appInfo = new AppInfo();

            try
            {
                if (process.MainModule.FileVersionInfo.CompanyName != null
                    && process.MainModule.FileVersionInfo.CompanyName.ToLower().Contains("microsoft"))
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
                return emptyAppInfo;
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

                    return emptyAppInfo;
                }
            }

            return appInfo;
        }

        public static AppInfo Create(string appName)
        {
            return new AppInfo()
            {
                Name = appName,
                Company = string.Empty,
                Version = string.Empty,
                FileName = string.Empty,
                Description = string.Empty,
                FullName = string.Empty
            };
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Name.GetHashCode();
                hash = hash * 23 + FullName.GetHashCode();
                hash = hash * 23 + FileName.GetHashCode();
                hash = hash * 23 + Company.GetHashCode();
                hash = hash * 23 + Description.GetHashCode();

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is AppInfo == false)
                return false;

            var other = (AppInfo)obj;

            return Equals(other);
        }

        private bool Equals(AppInfo other)
        {
            return this.Name == other.Name &&
                   this.FullName == other.FullName &&
                   this.FileName == other.FileName &&
                   this.Company == other.Company &&
                   this.Description == other.Description;
        }

        public static bool operator ==(AppInfo first, AppInfo second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(AppInfo first, AppInfo second)
        {
            return !first.Equals(second);
        }
    }
}
