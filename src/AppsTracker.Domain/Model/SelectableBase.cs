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
            IsSelectedChanging?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler IsSelectedChanging;
    }
}
