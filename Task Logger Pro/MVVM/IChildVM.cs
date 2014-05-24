using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.MVVM
{
    interface IChildVM 
    {
        string Title { get; }
        bool IsContentLoaded { get; }
        void LoadContent();
    }
}
