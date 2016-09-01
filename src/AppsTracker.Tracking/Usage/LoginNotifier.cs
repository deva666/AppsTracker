using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;
using Microsoft.Win32;
using AppsTracker.Domain.Util;
using AppsTracker.Common.Communication;

namespace AppsTracker.Tracking.Usage
{
    internal sealed class LoginNotifier : IUsageNotifier
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;
        private readonly Subject<UsageEvent> usageSubject = new Subject<UsageEvent>();

        public IObservable<UsageEvent> UsageObservable
        {
            get
            {
                return usageSubject.AsQbservable();
            }
        }

        public LoginNotifier()
        {
            SystemEvents.PowerModeChanged += PowerModeChanged;
        }

        private void PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    InitLogin();
                    mediator.NotifyColleagues(MediatorMessages.RESUME_TRACKING);
                    break;
                case PowerModes.StatusChange:
                    break;
                case PowerModes.Suspend:
                    mediator.NotifyColleagues(MediatorMessages.STOP_TRACKING);
                    break;
            }
        }

        public void Init()
        {
            InitLogin();   
        }

        private void InitLogin()
        {
            var user = GetUzer(Environment.UserName);
            var usageLogin = LoginUser(user.ID);
            //usageSubject.OnNext(usageLogin);

            //trackingService.Initialize(user.ToModel(), usageLogin.UsageID);
        }

        public Data.Models.Usage LoginUser(int userId)
        {
            var login = new Data.Models.Usage(userId, UsageTypes.Login) { UsageEnd = DateTime.Now, IsCurrent = true };
            repository.SaveNewEntity(login);
            return login;
        }

        private Uzer GetUzer(string name)
        {
            var uzer = repository.GetFiltered<Uzer>(u => u.Name == name).FirstOrDefault();
            if (uzer == null)
            {
                uzer = new Uzer(name);
                repository.SaveNewEntity(uzer);
            }
            return uzer;
        }
    }
}
