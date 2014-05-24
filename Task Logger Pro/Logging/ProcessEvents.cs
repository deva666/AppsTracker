using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Logging
{
    class ProcessEvents
    {
        private ManagementEventWatcher WatchForProcessStart( string processName )
        {
            string queryString =
                "SELECT TargetInstance" +
                "  FROM __InstanceCreationEvent " +
                "WITHIN  10 " +
                " WHERE TargetInstance ISA 'Win32_Process' " +
                "   AND TargetInstance.Name = '" + processName + "'";

            string scope = @"\\.\root\CIMV2";

            ManagementEventWatcher watcher = new ManagementEventWatcher( scope, queryString );
            watcher.EventArrived += ProcessStarted;
            watcher.Start();
            return watcher;
        }

        private ManagementEventWatcher WatchForProcessEnd( string processName )
        {
            string queryString =
                "SELECT TargetInstance" +
                "  FROM __InstanceDeletionEvent " +
                "WITHIN  10 " +
                " WHERE TargetInstance ISA 'Win32_Process' " +
                "   AND TargetInstance.Name = '" + processName + "'";

            string scope = @"\\.\root\CIMV2";

            ManagementEventWatcher watcher = new ManagementEventWatcher( scope, queryString );
            watcher.EventArrived += ProcessEnded;
            watcher.Start();
            return watcher;
        }

        private void ProcessEnded( object sender, EventArrivedEventArgs e )
        {
            ManagementBaseObject targetInstance = ( ManagementBaseObject ) e.NewEvent.Properties["TargetInstance"].Value;
            string processName = targetInstance.Properties["Name"].Value.ToString();
            Console.WriteLine( String.Format( "{0} process ended", processName ) );
        }

        private void ProcessStarted( object sender, EventArrivedEventArgs e )
        {
            ManagementBaseObject targetInstance = ( ManagementBaseObject ) e.NewEvent.Properties["TargetInstance"].Value;
            string processName = targetInstance.Properties["Name"].Value.ToString();
            Console.WriteLine( String.Format( "{0} process started", processName ) );
        }
    }
}
