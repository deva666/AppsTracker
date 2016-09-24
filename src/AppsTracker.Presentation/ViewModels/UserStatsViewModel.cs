#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using AppsTracker.Common.Communication;
using AppsTracker.Domain;
using AppsTracker.Domain.Usages;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class UserStatsViewModel : ViewModelBase
    {
        private readonly IUseCase<UserLoggedTime> userLoggedTimeUseCase;
        private readonly IUseCase<String, UsageOverview> usageOverviewUseCase;
        private readonly Mediator mediator;

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
            get
            {
                return returnFromDetailedViewCommand ??
                  (returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView));
            }
        }


        [ImportingConstructor]
        public UserStatsViewModel(IUseCase<UserLoggedTime> userLoggedTimeUseCase,
                                  IUseCase<String, UsageOverview> usageOverviewUseCase,
                                  Mediator mediator)
        {
            this.userLoggedTimeUseCase = userLoggedTimeUseCase;
            this.usageOverviewUseCase = usageOverviewUseCase;
            this.mediator = mediator;

            usersList = new TaskRunner<IEnumerable<UserLoggedTime>>(userLoggedTimeUseCase.Get, this);
            dailyUsageList = new TaskRunner<IEnumerable<UsageOverview>>(GetSubContent, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        private void ReloadAll()
        {
            usersList.Reload();
            dailyUsageList.Reload();
        }


        private IEnumerable<UsageOverview> GetSubContent()
        {
            var user = SelectedUser;
            if (user == null)
                return null;

            return usageOverviewUseCase.Get(user.Username);
        }

        private void ReturnFromDetailedView()
        {
            SelectedUser = null;
        }
    }
}
