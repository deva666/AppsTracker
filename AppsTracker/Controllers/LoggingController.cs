#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Logging;

namespace AppsTracker.Controllers
{
    [Export(typeof(ILoggingController))]
    internal sealed class LoggingController : ILoggingController
    {

        private readonly IList<IComponent> components = new List<IComponent>();

        public void Initialize(Setting settings)
        {
            components.Add(new WindowLogger(settings));
            components.Add(new UsageLogger(settings));
            components.Add(new LogCleaner(settings));
        }

        private void OnAll(Action<IComponent> action)
        {
            foreach (var comp in components)
                action(comp);
        }

        private void OnAllParallel(Action<IComponent> action)
        {
            Parallel.ForEach<IComponent>(components, action);
        }

        public void SettingsChanging(Setting settings)
        {
            foreach (var comp in components)
                comp.SettingsChanged(settings);
        }

        public void Dispose()
        {
            OnAll(l => l.Dispose());
        }
    }
}
