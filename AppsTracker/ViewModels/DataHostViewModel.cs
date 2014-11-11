using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.MVVM;
using AppsTracker.Pages.ViewModels;

namespace AppsTracker.ViewModels
{
    internal sealed class DataHostViewModel : HostViewModel, IChildVM
    {
        public string Title
        {
            get
            {
                return "data";
            }
        }

        public bool IsContentLoaded
        {
            get;
            private set;

        }

        public DataHostViewModel()
        {
            this.Register<Data_logsViewModel>(() => new Data_logsViewModel());
            this.Register<Data_keystrokesViewModel>(() => new Data_keystrokesViewModel());
            this.Register<Data_screenshotsViewModel>(() => new Data_screenshotsViewModel());
            this.Register<Data_dayViewModel>(() => new Data_dayViewModel());

            this.SelectedChild = Resolve(typeof(Data_logsViewModel));
        }

        public void LoadContent()
        {
            IsContentLoaded = true;
        }

        protected override void Disposing()
        {
            if (_selectedChild != null)
                ((ViewModelBase)_selectedChild).Dispose();
            _selectedChild = null;
            base.Disposing();
        }

        //protected override void ChangePage(object parameter)
        //{
        //    string viewName = parameter as string;
        //    if (viewName == null)
        //        return;
        //    switch (viewName)
        //    {
        //        case "APPS":
        //            SelectedChild = new Data_logsViewModel();
        //            break;
        //        case "KEYSTROKES":
        //            SelectedChild = new Data_keystrokesViewModel();
        //            break;
        //        case "SCREENSHOTS":
        //            SelectedChild = new Data_screenshotsViewModel();
        //            break;
        //        case "DAY VIEW":
        //            SelectedChild = new Data_dayViewModel();
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }
}
