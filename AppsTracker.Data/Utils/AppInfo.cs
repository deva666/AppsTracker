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
    public class AppInfo 
    {
        public virtual string Name { get; private set; }
        public string Version { get; private set; }
        public string Company { get; private set; }
        public string Description { get; private set; }
        public string FileName { get; private set; }
        public string FullName { get; private set; }

        private static Process GetProcessFromHandle(IntPtr hWnd)
        {
            uint processID = 0;
            if (hWnd != IntPtr.Zero)
            {
                WinAPI.GetWindowThreadProcessId(hWnd, out processID);
                if (processID != 0)
                    return System.Diagnostics.Process.GetProcessById(checked((int)processID));
            }
            return null;
        }

        public static AppInfo GetAppInfo(IntPtr hWnd)
        {
            var process = GetProcessFromHandle(hWnd);

            if (process == null)
                return null;

            var appInfo = new AppInfo();

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
            var other = obj as AppInfo;
            if (other == null)
                return false;

            return this.Name == other.Name &&
                   this.FullName == other.FullName &&
                   this.FileName == other.FileName &&
                   this.Company == other.Company &&
                   this.Description == other.Description;
        }

        public static bool operator ==(AppInfo first, AppInfo second)
        {
            if (object.ReferenceEquals(null, first))
            {
                return object.ReferenceEquals(null, second);
            }

            return first.Equals(second);
        }

        public static bool operator !=(AppInfo first, AppInfo second)
        {
            if (object.ReferenceEquals(null, first))
            {
                return !object.ReferenceEquals(null, second);
            }

            return !first.Equals(second);
        }
    }
}
