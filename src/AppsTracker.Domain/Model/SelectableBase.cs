using System;

namespace AppsTracker.Domain.Model
{
    public abstract class SelectableBase
    {
        protected bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                SelectedChanging();
            }
        }

        protected void SelectedChanging()
        {
            var handler = IsSelectedChanging;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        public event EventHandler IsSelectedChanging;
    }
}
