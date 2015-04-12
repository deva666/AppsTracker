using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics.CodeAnalysis;

namespace AppsTracker.MVVM
{
    public class DelegateCommandAsync : ICommand
    {
        private Func<Task> _delegate;
        private Func<object, Task> _parameterizedDelegate;
        private Func<bool> _canExecute;

        public DelegateCommandAsync(Func<Task> @delegate)
        {
            _delegate = @delegate;
        }

        public DelegateCommandAsync(Func<object, Task> parameterizedDelegate)
        {
            _parameterizedDelegate = parameterizedDelegate;
        }

        public DelegateCommandAsync(Func<Task> @delegate, Func<bool> canExecute)
            : this(@delegate)
        {
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public async void Execute(object parameter)
        {
            if (_parameterizedDelegate != null)
                await _parameterizedDelegate(parameter);
            else
                await _delegate();
        }
    }
}
