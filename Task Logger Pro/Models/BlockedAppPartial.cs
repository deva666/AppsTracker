using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class BlockedApp
    {
        public BlockedApp() { }
        public BlockedApp( int userID, int appID )
        {
            this.Date = DateTime.Now;
            this.UserID = userID;
            this.ApplicationID = appID;
        }
    }
}
