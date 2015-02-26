using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppsTracker.MVVM
{
   public class DelegateCommandAsync : ICommand
   {
      private Func<Task> _delegate;
      private Func<object, Task> _parameterizedDelegate;

      public DelegateCommandAsync(Func<Task> @delegate)
      {
         _delegate = @delegate;
      }

      public DelegateCommandAsync(Func<object, Task> parameterizedDelegate)
      {
         _parameterizedDelegate = parameterizedDelegate;
      }

      public bool CanExecute(object parameter)
      {
         return true;
      }

      public event EventHandler CanExecuteChanged;

      public async void Execute(object parameter)
      {
         if (_parameterizedDelegate != null)
            await _parameterizedDelegate(parameter);
         else
            await _delegate();
      }
   }
}
