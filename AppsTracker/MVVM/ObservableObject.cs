#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace AppsTracker.MVVM
{
    public class ObservableObject : INotifyPropertyChanged
    {
        protected void PropertyChanging(string propertyName)
        {
            var handler = Volatile.Read(ref PropertyChanged);
            if (handler != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool ThrowExceptionOnInvalidProperty { get; private set; }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public virtual void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                if (this.ThrowExceptionOnInvalidProperty) throw new Exception("Invalid property name: " + propertyName);
            }
        }

        protected void ClearPropertyChangedEventHandlers()
        {
            if (PropertyChanged != null)
            {
                var delegateBuffer = PropertyChanged.GetInvocationList();
                foreach (PropertyChangedEventHandler handler in delegateBuffer)
                {
                    PropertyChanged -= handler;
                }
            }
            this.PropertyChanged = null;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
