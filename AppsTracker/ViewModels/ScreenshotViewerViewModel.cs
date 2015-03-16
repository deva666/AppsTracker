#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;


namespace AppsTracker.ViewModels
{
    internal sealed class ScreenshotViewerViewModel : ViewModelBase
    {
        public event EventHandler CloseEvent;

        private int currentIndex;
        private int totalItemCount;
        private IEnumerable<Screenshot> screenshotCollection;

        public override string Title
        {
            get { return "Screenshots"; }
        }

        public int CurrentIndex
        {
            get { return currentIndex; }
            set
            {
                currentIndex = ++value;
                PropertyChanging("CurrentIndex");
            }
        }

        public int TotalItemCount
        {
            get { return totalItemCount; }
            set { SetPropertyValue(ref totalItemCount, value); }
        }

        public IEnumerable<Screenshot> ScreenshotCollection
        {
            get { return screenshotCollection; }
        }
        public ICommand CloseCommand
        {
            get { return new DelegateCommand(Close); }
        }

        public ScreenshotViewerViewModel()
        {

        }

        public ScreenshotViewerViewModel(IEnumerable<Screenshot> screenshotCollection)
            : this()
        {
            this.screenshotCollection = screenshotCollection;
            TotalItemCount = screenshotCollection.Count();
        }

        private void Close()
        {
            CloseEvent.InvokeSafely(this, EventArgs.Empty);
        }
    }
}
