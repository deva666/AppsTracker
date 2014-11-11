using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Logging
{
    public class IdleTimeWatcher
    {
        private IdleTimeWatcher() { }

        public static IdleTimeInfo GetIdleTimeInfo()
        {
            int systemUptime = Environment.TickCount,
                lastInputTicks = 0,
                idleTicks = 0;

            WinAPI.LASTINPUTINFO lastInputInfo = new WinAPI.LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            if (WinAPI.GetLastInputInfo(ref lastInputInfo))
            {
                lastInputTicks = (int)lastInputInfo.dwTime;

                idleTicks = systemUptime - lastInputTicks;
            }

            return new IdleTimeInfo
            {
                LastInputTime = DateTime.Now.AddMilliseconds(-1 * idleTicks),
                IdleTime = new TimeSpan(0, 0, 0, 0, idleTicks),
                SystemUptimeMilliseconds = systemUptime
            };
        }
    }

    public class IdleTimeInfo
    {
        public DateTime LastInputTime { get; internal set; }

        public TimeSpan IdleTime { get; internal set; }

        public int SystemUptimeMilliseconds { get; internal set; }
    }
}
