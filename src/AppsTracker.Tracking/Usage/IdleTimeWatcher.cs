using System;
using System.Runtime.InteropServices;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking
{
    public class IdleTimeWatcher
    {
        private IdleTimeWatcher() { }

        public static IdleTimeInfo GetIdleTimeInfo()
        {
            int systemUptime = Environment.TickCount;
            int lastInputTicks = 0;
            int idleTicks = 0;

            NativeMethods.LASTINPUTINFO lastInputInfo = new NativeMethods.LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            if (NativeMethods.GetLastInputInfo(ref lastInputInfo))
            {
                lastInputTicks = (int)lastInputInfo.dwTime;
                idleTicks = systemUptime - lastInputTicks;
            }
            else
            {
                System.Diagnostics.Debug.Fail("WinAPI.GetLastInputInfo failed");
            }

            return new IdleTimeInfo
            {
                LastInputTime = DateTime.Now.AddMilliseconds(-1 * idleTicks),
                IdleTime = new TimeSpan(0, 0, 0, 0, idleTicks),
                SystemUptimeMilliseconds = systemUptime
            };
        }
    }

    public struct IdleTimeInfo
    {
        public DateTime LastInputTime { get; internal set; }

        public TimeSpan IdleTime { get; internal set; }

        public int SystemUptimeMilliseconds { get; internal set; }
    }
}
