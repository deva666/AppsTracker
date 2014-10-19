using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class Window
    {
        public TimeSpan Duration { get { return this.GetWindowDuration( ); } }
    }
}
