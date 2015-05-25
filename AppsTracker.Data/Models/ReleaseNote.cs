using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Data.Models
{
    public class ReleaseNote
    {
        public string Version { get; private set; }
        public string Note { get; private set; }
    }
}
