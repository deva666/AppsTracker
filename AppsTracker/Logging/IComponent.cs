using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    interface IComponent : IDisposable
    {        
        void SettingsChanged(ISettings settings);
    }
}
