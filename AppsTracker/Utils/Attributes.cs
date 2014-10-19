using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class CleanUpAttribute : Attribute
    {
         readonly bool cleanUp;

        public CleanUpAttribute(bool cleanUp)
        {
            this.cleanUp = cleanUp;
        }

        public int NamedInt { get; set; }
    }
}
