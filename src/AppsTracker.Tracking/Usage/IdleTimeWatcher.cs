using System;
using System.Runtime.InteropServices;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking
{
    public class IdleTimeWatcher
    {
        private IdleTimeWatcher() { }

        public static TimeSpan GetIdleTimeSpan()
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
                return new TimeSpan(0, 0, 0, 0, idleTicks);
            }
            else
            {
                return TimeSpan.MinValue;
            }
        }
    }
}
