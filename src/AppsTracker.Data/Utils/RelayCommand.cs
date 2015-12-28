#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Windows.Input;

namespace AppsTracker.Data.Utils
{
   public class RelayCommand : ICommand
   {
      Action _action;
      Action<object> _parameterizedAction;
      Predicate<object> _canExecute;

      public RelayCommand(Action action)
      {
         _action = action;
      }

      public RelayCommand(Action<object> parameterizedAction)
      {
         _parameterizedAction = parameterizedAction;
      }

      public RelayCommand(Action action, Predicate<object> canExecute)
         : this(action)
      {
         _canExecute = canExecute;
      }

      public RelayCommand(Action<object> parameterizedAction, Predicate<object> canExecute)
         : this(parameterizedAction)
      {
         _canExecute = canExecute;
      }

      #region ICommand Members

      public bool CanExecute(object parameter)
      {
         return _canExecute == null ? true : _canExecute(parameter);
      }

      public event EventHandler CanExecuteChanged { add { } remove { } }

      public void Execute(object parameter)
      {
         if (_action != null) _action();
         if (_parameterizedAction != null) _parameterizedAction(parameter);
      }

      #endregion
   }

}
