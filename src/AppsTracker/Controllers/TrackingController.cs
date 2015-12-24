#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Tracking;

namespace AppsTracker.Controllers
{
    [Export(typeof(ITrackingController))]
    internal sealed class TrackingController : ITrackingController
    {
        private readonly IEnumerable<ITrackingModule> modules;

        [ImportingConstructor]
        public TrackingController([ImportMany]IEnumerable<ITrackingModule> modules)
        {
            this.modules = modules;
        }

        public void Initialize(Setting settings)
        {
            foreach (var module in modules.OrderBy(m => m.InitializationOrder))
            {
                module.Initialize(settings);
            }
        }

        public void SettingsChanging(Setting settings)
        {
            foreach (var module in modules)
            {
                module.SettingsChanged(settings);
            }
        }

        public void Dispose()
        {
            foreach (var module in modules)
            {
                module.Dispose();
            }
        }
    }
}
