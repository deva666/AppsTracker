#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppsTracker.DAL.Service;
using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    internal sealed class ComponentContainer : IDisposable
    {
        IList<IComponent> _components = new List<IComponent>();

        public ComponentContainer(ISettings settings)
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

        public void SettingsChanging(ISettings settings)
        {
            foreach (var comp in _components)
                comp.SettingsChanged(settings);
        }

        public void SetKeyboardHookEnabled(bool enabled)
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
