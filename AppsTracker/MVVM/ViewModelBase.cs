#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Diagnostics;

namespace AppsTracker.MVVM
{
    public abstract class ViewModelBase : ObservableObject, IWorker, IDisposable
    {
        protected bool working;

        protected object @lock = new object();

        public abstract string Title { get; }

        public bool Working
        {
            get
            {
                lock (@lock)
                    return working;
            }
            set
            {
                lock (@lock)
                {
                    SetPropertyValue(ref working, value);
                }
            }
        }

        public void Dispose()
        {
            Disposing();
        }

        protected virtual void Disposing()
        {
            Debug.WriteLine(string.Format("{0}, {1}, {2} Disposed", this.GetType().Name, this.GetType().FullName, this.GetHashCode()));
        }

#if DEBUG
        ~ViewModelBase()
        {
            Debug.WriteLine(string.Format("{0}, {1}, {2} Finalized", this.GetType().Name, this.GetType().FullName, this.GetHashCode()));
        }
#endif
    }

}
