using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Widgets.Behavior
{
    interface IDragable
    {
        Type DataType { get; }
        void Remove(object i);
    }
}
