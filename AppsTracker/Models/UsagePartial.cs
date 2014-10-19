using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class Usage
    {
        public bool IsSelected { get; set; }

        public TimeSpan Duration
        {
            get
            {
                return IsCurrent ? new TimeSpan(DateTime.Now.Ticks - UsageStart.Ticks) : new TimeSpan(UsageEnd.Ticks - UsageStart.Ticks);
            }
        }
   
        public Usage(int userID)
            
        {
            this.UsageStart = DateTime.Now;
            this.UserID = userID;
        }

        public Usage(int userID, int usageTypeID)
            : this(userID)
        {
            this.UsageTypeID = usageTypeID;
        }

    }
}
