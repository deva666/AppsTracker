#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Data.Service;
using AppsTracker.Common.Communication;

namespace AppsTracker.ViewModels
{
    [Export] 
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class UserStatsViewModel : ViewModelBase
    {
        private readonly IStatsService statsService;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;

        public override string Title
        {
            get { return "USERS"; }
        }


        public object SelectedItem { get; set; }


        private UserLoggedTime selectedUser;

        public UserLoggedTime SelectedUser
        {
            get { return selectedUser; }
            set
            {
                SetPropertyValue(ref selectedUser, value);
                if (selectedUser != null)
                    dailyUsageList.Reload();
            }
        }


        public UsageModel UsageModel { get; set; }


        private readonly AsyncProperty<IEnumerable<UserLoggedTime>> usersList;

        public AsyncProperty<IEnumerable<UserLoggedTime>> UsersList
        {
            get { return usersList; }
        }


        private readonly AsyncProperty<IEnumerable<UsageOverview>> dailyUsageList;

        public AsyncProperty<IEnumerable<UsageOverview>> DailyUsageList
        {
            get { return dailyUsageList; }
        }


        private ICommand returnFromDetailedViewCommand;

        public ICommand ReturnFromDetailedViewCommand
        {
            get { return returnFromDetailedViewCommand ?? (returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView)); }
        }


        [ImportingConstructor]
        public UserStatsViewModel(IStatsService statsService,
                                  ITrackingService trackingService,
                                  IMediator mediator)
        {
            this.statsService = statsService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            usersList = new AsyncProperty<IEnumerable<UserLoggedTime>>(GetContent, this);
            dailyUsageList = new AsyncProperty<IEnumerable<UsageOverview>>(GetSubContent, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        public void ReloadAll()
        {
            usersList.Reload();
            dailyUsageList.Reload();
        }


        private IEnumerable<UserLoggedTime> GetContent()
        {
            return statsService.GetAllUsers(trackingService.DateFrom, trackingService.DateTo);
        }


        private IEnumerable<UsageOverview> GetSubContent()
        {
            var user = SelectedUser;
            if (user == null)
                return null;

            return statsService.GetUsageSeries(user.Username, trackingService.DateFrom, trackingService.DateTo);
        }


        private void ReturnFromDetailedView()
        {
            SelectedUser = null;
        }
    }
}
