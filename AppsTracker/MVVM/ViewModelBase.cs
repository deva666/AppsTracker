#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AppsTracker.MVVM
{
    public abstract class ViewModelBase : ObservableObject, IWorker, IDisposable
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

        protected void SetPropertyValue<T>(ref T target, T value, [CallerMemberName] string caller = null)
        {
            if (object.Equals(target, value))
                return;
            target = value;
            PropertyChanging(caller);
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
