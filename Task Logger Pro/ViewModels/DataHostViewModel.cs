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
        }

        protected override void Disposing()
        {
            this._selectedChild = null;
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
                //case "FILELOGS":
                //    SelectedChild = new Data_filelogsViewModel();
                //    break;
                default:
                    break;
            }
        }
    }
}
