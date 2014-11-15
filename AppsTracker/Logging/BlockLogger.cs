#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Linq;

using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    internal sealed class BlockLogger : IComponent
    {
        bool _isLoggingEnabled;

        LazyInit<AppBlocker> _appBlocker;

        public BlockLogger()
        {
            Init();
        }

        private void Init()
        {
            _appBlocker = new LazyInit<AppBlocker>(() => new AppBlocker()
                                                        , a => a.AppBlocked += AppBlocked
                                                        , a => a.AppBlocked -= AppBlocked);
            var enabled = IsServiceEnabled();

            _appBlocker.Enabled = enabled;
        }

        public void SettingsChanged(ISettings settings)
        {

        }

        private bool IsServiceEnabled()
        {
            using (var context = new AppsEntities())
            {
                return context.AppsToBlocks.Any(a => a.UserID == Globals.UserID);
            }
        }

        private void AppBlocked(object sender, AppBlockerEventArgs args)
        {
            if (_isLoggingEnabled == false)
                return;

            using (var context = new AppsEntities())
            {
                context.BlockedApps.Add(new BlockedApp()
                {
                    Date = DateTime.Now,
                    ApplicationID = args.Aplication.ApplicationID,
                    UserID = Globals.UserID
                });
                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _appBlocker.Enabled = false;
        }


        public void SetComponentEnabled(bool enabled)
        {
            _isLoggingEnabled = enabled;
        }
    }
}
