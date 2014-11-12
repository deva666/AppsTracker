using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AppsTracker.MVVM;
using AppsTracker.Pages.ViewModels;

namespace AppsTracker.ViewModels
{
    internal sealed class StatisticsHostViewModel : HostViewModel
    {
        public override string Title { get { return "statistics"; } }

        public StatisticsHostViewModel()
        {
            Register<Statistics_usersViewModel>(() => new Statistics_usersViewModel());
            Register<Statistics_appUsageViewModel>(() => new Statistics_appUsageViewModel());
            Register<Statistics_dailyAppUsageViewModel>(() => new Statistics_dailyAppUsageViewModel());
            Register<Statistics_keystrokesViewModel>(() => new Statistics_keystrokesViewModel());
            Register<Statistics_screenshotsViewModel>(() => new Statistics_screenshotsViewModel());

            SelectedChild = Resolve(typeof(Statistics_usersViewModel));
        }
    }
}
