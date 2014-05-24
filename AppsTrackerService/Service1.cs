using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AppsTrackerService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            this.ServiceName = "apps tracker handler";
            this.CanStop = true;
            this.CanShutdown = true;
            this.CanPauseAndContinue = false;
            this.CanHandlePowerEvent = false;
            this.AutoLog = true;

        }

        protected override void OnStart( string[] args )
        {
            try
            {
                base.OnStart( args );
                string path = @"F:\Users\deva\Google disk\Projects SVN Repo\TaskLogger - Dev\Task Logger Pro\bin\Debug\Task Logger Pro.exe";
                Process.Start( path );
                this.Stop();
                
            }
            catch
            {
                this.Stop();
            }
            
        }

    }
}
