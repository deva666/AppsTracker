#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Screenshots;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class ScreenshotViewerViewModel : ViewModelBase
    {
        private int currentIndex;
        private int totalItemCount;
        private IEnumerable<Image> screenshotCollection;


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


        public IEnumerable<Image> ScreenshotCollection
        {
            get
            {
                return screenshotCollection;
            }
            set
            {
                screenshotCollection = value;
                TotalItemCount = screenshotCollection.Count();
            }
        }
    }
}
