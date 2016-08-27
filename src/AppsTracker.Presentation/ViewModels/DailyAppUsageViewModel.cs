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
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Data.Repository;
using AppsTracker.Common.Communication;
using AppsTracker.Domain.UseCases;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class DailyAppUsageViewModel : ViewModelBase
    {
        private readonly IRepository repository;
        private readonly IMediator mediator;
        private readonly IUseCase<AppDurationOverview> useCase;


        public override string Title
        {
            get { return "DAILY APP USAGE"; }
        }


        public object SelectedItem { get; set; }


        private readonly AsyncProperty<IEnumerable<AppDurationOverview>> appsList;

        public AsyncProperty<IEnumerable<AppDurationOverview>> AppsList
        {
            get { return appsList; }
        }


        [ImportingConstructor]
        public DailyAppUsageViewModel(IRepository repository,
                                      IUseCase<AppDurationOverview> useCase,
                                      IMediator mediator)
        {
            this.repository = repository;
            this.useCase = useCase;
            this.mediator = mediator;

            appsList = new TaskRunner<IEnumerable<AppDurationOverview>>(useCase.Get, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(appsList.Reload));
        }
    }
}
