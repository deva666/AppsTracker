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

using AppsTracker.Models.EntityModels;
using AppsTracker.MVVM;


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
