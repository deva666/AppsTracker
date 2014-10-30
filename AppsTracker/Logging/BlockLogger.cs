using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    internal sealed class BlockLogger : IComponent
    {
        ServiceWrap<AppBlocker> _appBlocker;
        IAppsService _service;

        public BlockLogger()
        {
            _service = ServiceFactory.Instance.GetService<IAppsService>();
            _appBlocker = new ServiceWrap<AppBlocker>(() => new AppBlocker()
                                                         , a => a.AppBlocked += AppBlocked
                                                         , a => a.AppBlocked -= AppBlocked);
            var enabled = _service.GetQueryable<AppsToBlock>()
                                    .Where(a => a.UserID == Globals.UserID)
                                    .Count() > 0;

            _appBlocker.Enabled = enabled;
        }
        public void SettingsChanged(ISettings settings)
        {

        }

        private void AppBlocked(object sender, AppBlockerEventArgs args)
        {
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
    }
}
