using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppsTracker.Common.Utils;
using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    internal sealed class BlockLogger : IComponent
    {
        bool _isLoggingEnabled;

        LazyInit<AppBlocker> _appBlocker;

        IAppsService _service;

        public BlockLogger()
        {
            _service = ServiceFactory.Get<IAppsService>();
            Init();
        }

        private void Init()
        {
            _appBlocker = new LazyInit<AppBlocker>(() => new AppBlocker()
                                                        , a => a.AppBlocked += AppBlocked
                                                        , a => a.AppBlocked -= AppBlocked);
            var enabled = _service.GetFiltered<AppsToBlock>(a => a.UserID == Globals.UserID)
                                                .Count() > 0;

            _appBlocker.Enabled = enabled;
        }

        public void SettingsChanged(ISettings settings)
        {

        }

        private void AppBlocked(object sender, AppBlockerEventArgs args)
        {
            if (_isLoggingEnabled == false)
                return;

            _service.Add<BlockedApp>(new BlockedApp()
            {
                Date = DateTime.Now,
                ApplicationID = args.Aplication.ApplicationID,
                UserID = Globals.UserID
            });
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
