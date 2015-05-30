#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking
{
    public interface ITrackingModule : IDisposable
    {
        void SettingsChanged(Setting settings);
        void Initialize(Setting settings);
        int InitializationOrder { get; }
    }
}
