#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AppsTracker.MVVM
{
    public abstract class ViewModelBase : ObservableObject, IDisposable
    {
        protected bool _working;

        protected object @lock = new object();

        public abstract string Title { get; }

        public bool Working
        {
            get
            {
                lock (@lock)
                    return _working;
            }
            set
            {
                lock (@lock)
                {
                    _working = value;
                    PropertyChanging("Working");
                }
            }
        }

        public ViewModelBase()
        {
            Debug.WriteLine(string.Format("{0}, {1}, {2} Constructed", this.GetType().Name, this.GetType().FullName, this.GetHashCode()));
        }

        protected async Task LoadAsync<T>(Func<T> getter, Action<T> onComplete, bool captureContext = true)
        {
            Working = true;
            T result = await Task<T>.Run(getter).ConfigureAwait(captureContext);
            onComplete(result);
            Working = false;
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
