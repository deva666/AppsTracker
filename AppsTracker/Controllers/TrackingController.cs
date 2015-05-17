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
using AppsTracker;
using AppsTracker.Data.Models;
using AppsTracker.Tracking;

namespace AppsTracker.Controllers
{
    [Export(typeof(ITrackingController))]
    internal sealed class TrackingController : ITrackingController
    {
        private readonly IEnumerable<IModule> modules;

        [ImportingConstructor]
        public TrackingController([ImportMany]IEnumerable<IModule> modules)
        {
            this.modules = modules;
        }

        public void Initialize(Setting settings)
        {
            modules.ForEachAction(m => m.InitializeComponent(settings));
        }

        public void SettingsChanging(Setting settings)
        {
            modules.ForEachAction(m => m.SettingsChanged(settings));
        }

        public void Dispose()
        {
            modules.ForEachAction(m => m.Dispose());
        }
    }
}
