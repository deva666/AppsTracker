using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class UsageType
    {
        public UsageType( UsageTypes usageType )
        {
            this.UType = usageType.ToString();
        }
    }
}
