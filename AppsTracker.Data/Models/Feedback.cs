using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Data.Models
{
    public class Feedback
    {
        public string Description { get; private set; }
        public string StackTrace { get; private set; }
        public string ReporterEmail { get; private set; }
    }
}
