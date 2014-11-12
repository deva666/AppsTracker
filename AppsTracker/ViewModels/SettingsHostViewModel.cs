using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class SettingsHostViewModel : HostViewModel
    {
        public override string Title
        {
            get { return "settings"; }
        }

        public SettingsHostViewModel()
        {
            Register<Settings_generalViewModel>(() => new Settings_generalViewModel());
            Register<Settings_licenseViewModel>(() => new Settings_licenseViewModel());

            SelectedChild = Resolve(typeof(Settings_generalViewModel));
        }
    }
}
