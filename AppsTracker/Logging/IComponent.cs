#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;

using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    interface IComponent : IDisposable
    {
        void SettingsChanged(ISettings settings);
        void SetComponentEnabled(bool enabled);
    }
}
