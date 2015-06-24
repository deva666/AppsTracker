using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Data.Models
{
    public sealed class Feedback
    {
        public string Description { get; private set; }
        public string StackTrace { get; private set; }
        public string ReporterEmail { get; private set; }

        public Feedback(string description, string stackTrace, string reporterEmail = "")
        {
            Description = description;
            StackTrace = stackTrace;
            ReporterEmail = reporterEmail;
        }
    }
}
