using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Task_Logger_Pro.Models;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class ScreenshotViewerViewModel : ViewModelBase
    {
        public event EventHandler CloseEvent;

        IEnumerable<Screenshot> _screenshotCollection;
        
        public IEnumerable<Screenshot> ScreenshotCollection { get { return _screenshotCollection; } }
        public ICommand CloseCommand { get { return new DelegateCommand(Close); } }

        public ScreenshotViewerViewModel()
        {

        }

        public ScreenshotViewerViewModel(IEnumerable<Screenshot> screenshotCollection)
            : this()
        {
            _screenshotCollection = screenshotCollection;
        }

        private void Close()
        {
            if (CloseEvent != null) CloseEvent(this, EventArgs.Empty);
        }
    }
}
