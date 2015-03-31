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
using AppsTracker.Data.Models;
using AppsTracker.Logging;

namespace AppsTracker.Controllers
{
    [Export(typeof(ILoggingController))]
    internal sealed class LoggingController : ILoggingController
    {
#pragma warning disable 0649

        [ImportMany(typeof(IComponent))]
        private List<IComponent> components;

#pragma warning restore 0649

        public void Initialize(Setting settings)
        {
            ComponentsForEach(c => c.InitializeComponent(settings));
        }

        public void SettingsChanging(Setting settings)
        {
            ComponentsForEach(c => c.SettingsChanged(settings));
        }

        private void ComponentsForEach(Action<IComponent> action)
        {
            foreach (var comp in components)
                action(comp);
        }

        public void Dispose()
        {
            ComponentsForEach(c => c.Dispose());
        }
    }
}
