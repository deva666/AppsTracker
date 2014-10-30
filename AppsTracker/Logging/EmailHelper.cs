using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    internal sealed class EmailHelper : IComponent
    {
        ServiceWrap<EmailService> _emailService;

        public EmailHelper(ISettings settings)
        {
            _emailService = new ServiceWrap<EmailService>(() => new EmailService());
            Configure(settings);
        }


        private void Configure(ISettings settings)
        {
            _emailService.Enabled = settings.EnableEmailReports;
        }

        public void SettingsChanged(Models.Proxy.ISettings settings)
        {
            Configure(settings);
        }

        public void Dispose()
        {
            _emailService.Enabled = false;
        }
    }
}
