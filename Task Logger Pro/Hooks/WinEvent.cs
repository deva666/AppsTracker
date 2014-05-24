using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Task_Logger_Pro.Models;

namespace Task_Logger_Pro
{
    public sealed class WinEvent : IDisposable
    {
        #region Fields
        public event EventHandler<WinEventArgs> ActiveWindowChanged;

        WinEventCallBack winEventCallBack;
        bool isDisposed;
        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;
        const uint EVENT_SYSTEM_CAPTURESTART = 0x0008;
        IntPtr hookID = IntPtr.Zero;
        internal delegate void WinEventCallBack(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        #endregion

        #region Constructor

        public WinEvent()
        {
            if (App.Current.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
                SetHookSameThread();
            else
                SetHook();
        }

        private void SetHookSameThread()
        {
            App.Current.Dispatcher.Invoke(SetHook);
        }

        private void SetHook()
        {
            winEventCallBack = new WinEventCallBack(WinEventProc);
            hookID = Win32.SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winEventCallBack, 0, 0, WINEVENT_OUTOFCONTEXT);
        }


        #endregion

        #region ClassMethods

        public WinEventArgs GetWinEventArgs()
        {
            IntPtr hWnd = WinAPI.GetForegroundWindow();
            return new WinEventArgs(GetActiveWindowName(), GetProcess(hWnd));
        }

        public string GetActiveWindowName()
        {
            IntPtr foregroundWindow = WinAPI.GetForegroundWindow();
            StringBuilder windowTitle = new StringBuilder(WinAPI.GetWindowTextLength(foregroundWindow) + 1);
            if (WinAPI.GetWindowText(foregroundWindow, windowTitle, windowTitle.Capacity) > 0)
            {
                if (string.IsNullOrEmpty(windowTitle.ToString().Trim())) return "No Title";
                return windowTitle.ToString();
            }
            return "No Title";
        }

        private Process GetProcess(IntPtr hWnd)
        {
            uint processID = 0;
            if (hWnd != IntPtr.Zero)
            {
                WinAPI.GetWindowThreadProcessId(hWnd, out processID);
                if (processID != 0)
                {
                    return System.Diagnostics.Process.GetProcessById(Convert.ToInt32(processID));
                }
            }
            return null;
        }

        #endregion

        #region CallBackMethod

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hWnd != IntPtr.Zero)
            {
                StringBuilder windowTitleBuilder = new StringBuilder(WinAPI.GetWindowTextLength(hWnd) + 1);
                WinAPI.GetWindowText(hWnd, windowTitleBuilder, windowTitleBuilder.Capacity);
                var handler = ActiveWindowChanged;
                if (handler != null)
                    handler(this, new WinEventArgs(string.IsNullOrEmpty(windowTitleBuilder.ToString()) ? "No Title" : windowTitleBuilder.ToString(), GetProcess(hWnd)));
            }

        }

        #endregion

        #region IDisposable Members
        ~WinEvent()
        {
            Dispose(false);
            Debug.WriteLine("WinEvent Finalizer called");

        }

        private void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine("Disposing " + this.GetType().Name + " " + this.GetType().FullName);

            if (isDisposed) return;
            WinAPI.UnhookWinEvent(hookID);
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        internal class Win32
        {
            [DllImport("user32.dll")]
            public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventCallBack lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
        }

    }

    public class ProcessInfo
    {
        #region Properties

        public string ProcessName { get; private set; }
       // public string ProcessExe { get; private set; }
        public string ProcessVersion { get; private set; }
        public string ProcessCompany { get; private set; }
        public string ProcessDescription { get; private set; }
        public string ProcessFileName { get; private set; }
      //  public string ProcessComments { get; private set; }
        public string ProcessRealName { get; private set; }

        #endregion

        #region Construcor

        public ProcessInfo()
        {
        }

        public static ProcessInfo GetProcessInfo(Process process)
        {
            if (process == null)
                return null;
            ProcessInfo processInfo = new ProcessInfo();

            try
            {
                if (process.MainModule.FileVersionInfo.CompanyName != null && process.MainModule.FileVersionInfo.CompanyName.ToLower().Contains("microsoft"))
                {
                   // processInfo.ProcessExe = process.MainModule.FileName ?? "";
                    processInfo.ProcessVersion = process.MainModule.FileVersionInfo.ProductVersion ?? "";
                    processInfo.ProcessCompany = process.MainModule.FileVersionInfo.CompanyName ?? "";
                    processInfo.ProcessFileName = process.MainModule.FileVersionInfo.FileName ?? "";
                   // processInfo.ProcessComments = process.MainModule.FileVersionInfo.Comments ?? "";
                    if (string.IsNullOrEmpty(process.MainModule.FileVersionInfo.FileDescription))
                    {
                        processInfo.ProcessName = process.MainModule.FileVersionInfo.ProductName ?? "";
                        processInfo.ProcessDescription = process.MainModule.FileVersionInfo.FileDescription ?? "";
                    }
                    else
                    {
                        processInfo.ProcessName = process.MainModule.FileVersionInfo.FileDescription ?? "";
                        processInfo.ProcessDescription = process.MainModule.FileVersionInfo.ProductName ?? "";
                    }

                    processInfo.ProcessRealName = process.ProcessName ?? "";
                }
                else
                {
                    //processInfo.ProcessExe = process.MainModule.FileName ?? "";
                    processInfo.ProcessVersion = process.MainModule.FileVersionInfo.ProductVersion ?? "";
                    processInfo.ProcessCompany = process.MainModule.FileVersionInfo.CompanyName ?? "";
                    processInfo.ProcessFileName = process.MainModule.FileVersionInfo.FileName ?? "";
                    //processInfo.ProcessComments = process.MainModule.FileVersionInfo.Comments ?? "";
                    processInfo.ProcessName = process.MainModule.FileVersionInfo.ProductName ?? "";
                    processInfo.ProcessDescription = process.MainModule.FileVersionInfo.FileDescription ?? "";
                    processInfo.ProcessRealName = process.ProcessName ?? "";
                }
            }
            catch (Exception)
            {
               // processInfo.ProcessExe = "";
                processInfo.ProcessVersion = "";
                processInfo.ProcessCompany = "";
                processInfo.ProcessFileName = "";
               // processInfo.ProcessComments = "System process";
                processInfo.ProcessName = process.ProcessName;
                processInfo.ProcessDescription = "";
                processInfo.ProcessRealName = "";
            }

            return processInfo;

        }

        //    public ProcessInfo(Process process)
        //    {
        //        try
        //        {
        //            if (process != null)
        //            {
        //                if (process.MainModule.FileVersionInfo.CompanyName != null && process.MainModule.FileVersionInfo.CompanyName.ToLower().Contains("microsoft"))
        //                {
        //                    _processExe = process.MainModule.FileName ?? "";
        //                    _processVersion = process.MainModule.FileVersionInfo.ProductVersion ?? "";
        //                    _processCompany = process.MainModule.FileVersionInfo.CompanyName ?? "";
        //                    _processFileName = process.MainModule.FileVersionInfo.FileName ?? "";
        //                    _processComments = process.MainModule.FileVersionInfo.Comments ?? "";
        //                    if (string.IsNullOrEmpty(process.MainModule.FileVersionInfo.FileDescription))
        //                    {
        //                        _processName = process.MainModule.FileVersionInfo.ProductName ?? "";
        //                        _processDescription = process.MainModule.FileVersionInfo.FileDescription ?? "";
        //                    }
        //                    else
        //                    {
        //                        _processName = process.MainModule.FileVersionInfo.FileDescription ?? "";
        //                        _processDescription = process.MainModule.FileVersionInfo.ProductName ?? "";
        //                    }

        //                    _processRealName = process.ProcessName ?? "";
        //                }
        //                else
        //                {
        //                    _processExe = process.MainModule.FileName ?? "";
        //                    _processVersion = process.MainModule.FileVersionInfo.ProductVersion ?? "";
        //                    _processCompany = process.MainModule.FileVersionInfo.CompanyName ?? "";
        //                    _processFileName = process.MainModule.FileVersionInfo.FileName ?? "";
        //                    _processComments = process.MainModule.FileVersionInfo.Comments ?? "";
        //                    _processName = process.MainModule.FileVersionInfo.ProductName ?? "";
        //                    _processDescription = process.MainModule.FileVersionInfo.FileDescription ?? "";
        //                    _processRealName = process.ProcessName ?? "";
        //                }
        //            }
        //            else
        //            {
        //                _processExe = null;
        //                _processVersion = null;
        //                _processCompany = null;
        //                _processDescription = null;
        //                _processFileName = null;
        //                _processComments = null;
        //                _processName = null;
        //            }
        //        }

        //        catch (Exception ex)
        //        {
        //            Exceptions.Logger.DumpExceptionInfo(ex);
        //            Exceptions.Logger.DumpDebug(string.Format("Exception trying to read process, {0}{1}", process.ToString(), Environment.NewLine));
        //            _processExe = "";
        //            _processVersion = "";
        //            _processCompany = "";
        //            _processDescription = "";
        //            _processFileName = "";
        //            _processComments = "";
        //            _processName = process.ToString();
        //        }
        //    }

        #endregion

    }

    public class WinEventArgs : EventArgs
    {

        #region Properties

        public string WindowTitle { get; private set; }
        public ProcessInfo ProcessInfo { get; private set; }

        #endregion

        #region Constructor

        public WinEventArgs(string windowTitle, Process process)
        {
            ProcessInfo = ProcessInfo.GetProcessInfo(process);
            WindowTitle = windowTitle;
        }

        #endregion
    }
}

