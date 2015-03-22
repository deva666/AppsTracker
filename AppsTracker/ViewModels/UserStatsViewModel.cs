#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class UserStatsViewModel : ViewModelBase, ICommunicator
    {
        private readonly IStatsService statsService;

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

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        public UserStatsViewModel()
        {
            statsService = ServiceFactory.Get<IStatsService>();

            usersList = new AsyncProperty<IEnumerable<UserLoggedTime>>(GetContent, this);
            dailyUsageList = new AsyncProperty<IEnumerable<UsageOverview>>(GetSubContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadAll));
        }

        public void ReloadAll()
        {
            usersList.Reload();
            dailyUsageList.Reload();
        }

        private IEnumerable<UserLoggedTime> GetContent()
        {
            return statsService.GetAllUsers(Globals.DateFrom, Globals.DateTo);
        }

        private IEnumerable<UsageOverview> GetSubContent()
        {
            var model = SelectedUser;
            if (model == null)
                return null;

            return statsService.GetUsageSeries(model.Username, Globals.DateFrom, Globals.DateTo);
        }

        private void ReturnFromDetailedView()
        {
            SelectedUser = null;
        }
    }
}
