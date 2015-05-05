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
using AppsTracker.Tracking;

namespace AppsTracker.Controllers
{
    [Export(typeof(ITrackingController))]
    internal sealed class TrackingController : ITrackingController
    {
#pragma warning disable 0649

        [ImportMany(typeof(IModule))]
        private List<IModule> modules;

#pragma warning restore 0649

        public void Initialize(Setting settings)
        {
            ModulesForEach(m => m.InitializeComponent(settings));
        }

        public void SettingsChanging(Setting settings)
        {
            ModulesForEach(m => m.SettingsChanged(settings));
        }

        private void ModulesForEach(Action<IModule> action)
        {
            foreach (var module in modules)
                action(module);
        }

        public void Dispose()
        {
            ModulesForEach(m => m.Dispose());
        }
    }
}
