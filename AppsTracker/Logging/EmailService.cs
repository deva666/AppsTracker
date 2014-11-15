#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    internal sealed class EmailService : IComponent
    {
        LazyInit<EmailHelper> _emailService;

        public EmailService(ISettings settings)
        {
            _emailService = new LazyInit<EmailHelper>(() => new EmailHelper());
            Configure(settings);
        }


        private void Configure(ISettings settings)
        {
            _emailService.Enabled = settings.EnableEmailReports;
            if (settings.EnableEmailReports == false)
                return;
            _emailService.Component.EmailFrom = settings.EmailFrom;
            _emailService.Component.EmailTo = settings.EmailTo;
            _emailService.Component.Interval = settings.EmailInterval;
            _emailService.Component.SmtpHost = settings.EmailSmtpHost;
            _emailService.Component.SmtpPassword = settings.EmailSmtpPassword;
            _emailService.Component.SmtpPort = settings.EmailSmtpPort;
            _emailService.Component.SmtpUsername = settings.EmailSmtpUsername;
            _emailService.Component.SSL = settings.EmailSSL;
        }

        public void SettingsChanged(Models.Proxy.ISettings settings)
        {
            Configure(settings);
        }

        public void Dispose()
        {
            _emailService.Enabled = false;
        }


        public void SetComponentEnabled(bool enabled)
        {
            //do nothing
        }
    }
}
