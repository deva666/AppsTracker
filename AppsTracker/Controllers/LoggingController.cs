using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Logging;

namespace AppsTracker.Controllers
{
    [Export(typeof(ILoggingController))]
    internal sealed class LoggingController : ILoggingController
    {

        IList<IComponent> _components = new List<IComponent>();

        public void Initialize(Setting settings)
        {
            _components.Add(new WindowLogger(settings));
            _components.Add(new UsageLogger(settings));
            _components.Add(new AppBlockLogger());
            _components.Add(new EmailService(settings));
            _components.Add(new LogCleaner(settings));
        }

        private void OnAll(Action<IComponent> action)
        {
            foreach (var comp in _components)
                action(comp);
        }

        private void OnAllParallel(Action<IComponent> action)
        {
            Parallel.ForEach<IComponent>(_components, action);
        }

        public void SettingsChanging(Setting settings)
        {
            foreach (var comp in _components)
                comp.SettingsChanged(settings);
        }

        public void ToggleKeyboardHook(bool enabled)
        {
            var windowLogger = _components.FirstOrDefault(c => c.GetType() == typeof(WindowLogger)) as WindowLogger;
            if (windowLogger != null)
                windowLogger.SetKeyboardHookEnabled(enabled);
        }

        public void Dispose()
        {
            OnAll(l => l.Dispose());    
        }
    }
}
