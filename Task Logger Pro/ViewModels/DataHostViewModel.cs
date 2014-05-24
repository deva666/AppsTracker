using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Task_Logger_Pro.MVVM;
using Task_Logger_Pro.Pages.ViewModels;

namespace Task_Logger_Pro.ViewModels
{
    class DataHostViewModel : HostViewModel, IChildVM
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

        public void LoadContent()
        {
            this.SelectedChild = new Data_logsViewModel();
            IsContentLoaded = true;
            //Data_logsViewModel logsViewModel = new Data_logsViewModel();
            //Data_keystrokesViewModel keystrokesViewModel = new Data_keystrokesViewModel();
            //Data_screenshotsViewModel screenshotsViewModel = new Data_screenshotsViewModel();
            //Data_filelogsViewModel filelogsViewModel = new Data_filelogsViewModel();
            ////Data_computerUsageViewModel computerUsageViewModel = new Data_computerUsageViewModel();
            //Data_dayViewModel dayViewModel = new Data_dayViewModel();

            //var tempChildren = new List<IChildVM>() {
            //     logsViewModel,  keystrokesViewModel, screenshotsViewModel, dayViewModel, filelogsViewModel
            //};

            //this.Children = new ReadOnlyCollection<IChildVM>(tempChildren);
        }

        protected override void Disposing()
        {
            //foreach (var child in this.Children)
            //{
            //    ((ViewModelBase)child).Dispose();
            //}

            this._selectedChild = null;
            //this.Children = null;
            base.Disposing();
        }

        protected override void ChangePage(object parameter)
        {
            string viewName = parameter as string;
            if (viewName == null)
                return;
            switch (viewName)
            {
                case "APPS":
                    SelectedChild = new Data_logsViewModel();
                    break;
                case "KEYSTROKES":
                    SelectedChild = new Data_keystrokesViewModel();
                    break;
                case "SCREENSHOTS":
                    SelectedChild = new Data_screenshotsViewModel();
                    break;
                case "DAY VIEW":
                    SelectedChild = new Data_dayViewModel();
                    break;
                default:
                    break;
            }
            //if (viewName == "APPS")
            //    SelectedChild = new Data_logsViewModel();
            //else if (viewName == "DAY VIEW")
            //    SelectedChild = new Data_dayViewModel();
        }
    }
}
