using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Common
{
    public class MultiMap<K> 
    {
        private Dictionary<object, Func<K>> _map = new Dictionary<object, Func<K>>();
    }
}
