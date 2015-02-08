using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.Controllers
{
    public interface ILoggingController : IDisposable
    {
        void Initialize(Setting settings);
        void SettingsChanging(Setting settings);
        void ToggleKeyboardHook(bool enabled);
    }
}
