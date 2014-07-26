using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Task_Logger_Pro.MVVM
{
    abstract class HostViewModel : ViewModelBase
    {
        protected IChildVM _selectedChild;

        protected ICommand _changePageCommand;

        public virtual IChildVM SelectedChild
        {
            get
            {
                return _selectedChild;
            }
            set
            {
                if (_selectedChild != null && _selectedChild.Title == value.Title)
                    return;
                if (_selectedChild != null)
                    ((ViewModelBase)_selectedChild).Dispose();
                _selectedChild = value;
                PropertyChanging("SelectedChild");
                LoadChildContent();
            }
        }

        public virtual ICommand ChangePageCommand
        {
            get
            {
                return _changePageCommand == null ? _changePageCommand = new DelegateCommand(ChangePage) : _changePageCommand;
            }
        }

        protected abstract void ChangePage(object parameter);

        protected virtual void LoadChildContent()
        {
            if (this.SelectedChild != null)
            {
                if (!this.SelectedChild.IsContentLoaded)
                    this.SelectedChild.LoadContent();
            }
        }


    }
}
