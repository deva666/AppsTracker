using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppsTracker.Models.Utils
{
    public class ObservableObject : INotifyPropertyChanged
    {
        protected void PropertyChanging(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
