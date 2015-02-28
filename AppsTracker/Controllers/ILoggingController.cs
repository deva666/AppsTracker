using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Controllers
{
    public interface ILoggingController : IDisposable
    {
        void Initialize(Setting settings);
        void SettingsChanging(Setting settings);
        void ToggleKeyboardHook(bool enabled);
    }
}
