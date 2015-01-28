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
using System.Linq;

using AppsTracker.Models.EntityModels;
using AppsTracker.MVVM;


namespace AppsTracker.Pages.ViewModels
{
    internal sealed class ScreenshotViewerViewModel : ViewModelBase
    {
        public event EventHandler CloseEvent;

        private int _currentIndex;
        private int _totalItemCount;
        private IEnumerable<Screenshot> _screenshotCollection;

        public override string Title
        {
            get { return "Screenshots"; }
        }

        public int CurrentIndex
        {
            get
            {
                return _currentIndex;
            }
            set
            {
                _currentIndex = ++value;
                PropertyChanging("CurrentIndex");
            }
        }

        public int TotalItemCount
        {
            get
            {
                return _totalItemCount;
            }
            set
            {
                _totalItemCount = value;
                PropertyChanging("TotalItemCount");
            }
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
            TotalItemCount = _screenshotCollection.Count();
        }

        private void Close()
        {
            CloseEvent.InvokeSafely(this, EventArgs.Empty);
        }
    }
}
