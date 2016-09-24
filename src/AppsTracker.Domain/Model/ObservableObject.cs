using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AppsTracker.Domain.Model
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        protected void PropertyChanging(string propertyName)
        {
            var handler = Volatile.Read(ref PropertyChanged);
            if (handler != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetPropertyValue<T>(ref T target, T value, [CallerMemberName] string caller = null)
        {
            if (object.Equals(target, value))
                return;
            target = value;
            PropertyChanging(caller);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
