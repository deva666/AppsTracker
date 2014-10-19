using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.MVVM
{
    interface IChildVM 
    {
        string Title { get; }
        bool IsContentLoaded { get; }
        void LoadContent();
    }
}
