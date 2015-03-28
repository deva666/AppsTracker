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
        [ImportMany(typeof(IComponent))]
        private List<IComponent> components;


        public void Initialize(Setting settings)
        {
            ComponentsForEach(c => c.InitializeComponent(settings));
        }

        private void ComponentsForEach(Action<IComponent> action)
        {
            foreach (var comp in components)
                action(comp);
        }

        public void SettingsChanging(Setting settings)
        {
            foreach (var comp in components)
                comp.SettingsChanged(settings);
        }

        public void Dispose()
        {
            ComponentsForEach(l => l.Dispose());
        }
    }
}
