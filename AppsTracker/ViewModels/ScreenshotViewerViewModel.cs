using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using AppsTracker.MVVM;
using AppsTracker.Models.EntityModels;


namespace AppsTracker.Pages.ViewModels
{
    internal sealed class ScreenshotViewerViewModel : ViewModelBase
    {
        public event EventHandler CloseEvent;

        private IEnumerable<Screenshot> _screenshotCollection;

        public override string Title
        {
            get { return "Screenshots"; }
        }   

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
            CloseEvent.InvokeSafely(this, EventArgs.Empty);
        }
    }
}
