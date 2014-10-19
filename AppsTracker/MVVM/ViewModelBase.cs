using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Logging;
using AppsTracker.Pages.ViewModels;

namespace AppsTracker.MVVM
{
    public abstract class ViewModelBase : ObservableObject, IDisposable
    {

        public ViewModelBase()
        {
            Debug.WriteLine(string.Format("{0}, {1}, {2} Constructed", this.GetType().Name, this.GetType().FullName, this.GetHashCode()));
        }

        public void Dispose()
        {
            Disposing();
        }

        protected virtual void Disposing()
        {
            //ClearPropertyChangedEventHandlers();
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
