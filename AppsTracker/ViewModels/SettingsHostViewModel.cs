﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class SettingsHostViewModel : HostViewModel, IChildVM
    {
        public string Title
        {
            get { return "settings"; }
        }

        public bool IsContentLoaded
        {
            get;
            private set;
        }

        public SettingsHostViewModel()
        {
            Register<Settings_generalViewModel>(() => new Settings_generalViewModel());
            Register<Settings_licenseViewModel>(() => new Settings_licenseViewModel());

            SelectedChild = Resolve(typeof(Settings_generalViewModel));
        }

        public void LoadContent()
        {
            
            this.IsContentLoaded = true;
        }

        //protected override void ChangePage(object parameter)
        //{
        //    string viewName = parameter as string;
        //    if (viewName == null)
        //        return;

        //    switch (viewName)
        //    {
        //        case "APP SETTINGS":
        //            SelectedChild = new Settings_generalViewModel();
        //            break;
        //        case "LICENCE":
        //            SelectedChild = new Settings_licenseViewModel();
        //            break;
        //        default:
        //            break;
        //    }

        //}

    }
}