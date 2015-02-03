#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Windows.Input;

namespace AppsTracker.MVVM
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

        public DelegateCommand(Action<object> parameterizedAction, Predicate<object> canExecute)
            : this(parameterizedAction)
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
}
