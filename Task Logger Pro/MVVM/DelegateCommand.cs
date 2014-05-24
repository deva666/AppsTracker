using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Task_Logger_Pro.MVVM
{
    public class DelegateCommand : ICommand
    {
        Action _action;
        Action<object> _parameterizedAction;
        Predicate<object> _canExecute;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public DelegateCommand(Action<object> parameterizedAction)
        {
            _parameterizedAction = parameterizedAction;
        }

        public DelegateCommand(Action action, Predicate<object> canExecute)
            : this(action)
        {
            _canExecute = canExecute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_action != null) _action();
            if (_parameterizedAction != null) _parameterizedAction(parameter);
        }

        #endregion
    }

    public class DelegateCommand<T> : ICommand where T : System.Windows.FrameworkElement
    {
        Action<T> _action;

        public DelegateCommand(Action<T> action)
        {
            _action = action;
        }
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action(parameter as T);
        }

        #endregion
    }
}
