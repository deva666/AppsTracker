#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Diagnostics;
using AppsTracker.ServiceLocation;

namespace AppsTracker.MVVM
{
    public abstract class ViewModelBase : ObservableObject, IWorker
    {
        private bool working;

        private object _lock = new object();

        public abstract string Title { get; }

        public bool Working
        {
            get
            {
                lock (_lock)
                    return working;
            }
            set
            {
                lock (_lock)
                {
                    SetPropertyValue(ref working, value);
                }
            }
        }

#if DEBUG
        ~ViewModelBase()
        {
            Debug.WriteLine(string.Format("{0}, {1}, {2} Finalized", this.GetType().Name, this.GetType().FullName, this.GetHashCode()));
        }
#endif
    }

}
