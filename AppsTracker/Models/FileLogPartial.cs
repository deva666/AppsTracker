using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class FileLog
    {
        public FileLog( )
        {
            this.Date = DateTime.Now;
            this.NewPath = "";
        }

        public FileLog( string oldPath, string eventType, int userID )
            : this( )
        {
            this.Path = oldPath;
            this.Event = eventType;
            this.UserID = userID;
        }

        public FileLog( string oldPath, string eventType, string newPath, int userID )
            : this( oldPath, eventType, userID )
        {
            this.NewPath = newPath;
        }
    }
}
