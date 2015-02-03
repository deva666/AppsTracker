#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Common.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AppsTracker.MVVM
{
   internal abstract class HostViewModel : ViewModelBase
   {
      private Dictionary<object, ViewModelConnector> _childrenMap = new Dictionary<object, ViewModelConnector>();

      protected ViewModelBase _selectedChild;

      protected ICommand _changePageCommand;

      public ViewModelBase SelectedChild
      {
         get
         {
            return _selectedChild;
         }
         set
         {
            if (_selectedChild != null && _selectedChild.Title == value.Title)
               return;

            _selectedChild = value;
            PropertyChanging("SelectedChild");
         }
      }

      public virtual ICommand ChangePageCommand
      {
         get
         {
            return _changePageCommand ?? (_changePageCommand = new DelegateCommand(ChangePage));
         }
      }

      protected virtual void ChangePage(object parameter)
      {
         SelectedChild = Resolve(parameter);
      }

      protected void Register<T>(Func<T> getter) where T : ViewModelBase
      {
         Ensure.NotNull(getter);
         Ensure.Condition<InvalidOperationException>(_childrenMap.ContainsKey(typeof(T)) == false, string.Format("Type {0} is already bound!", typeof(T)));

         var connector = new ViewModelConnector(getter);
         _childrenMap.Add(typeof(T), connector);
      }

      protected ViewModelBase Resolve(object type)
      {
         Ensure.NotNull(type);
         Ensure.Condition<InvalidOperationException>(_childrenMap.ContainsKey(type) == true, string.Format("Can't resolve {0} type!", type));

         var connector = _childrenMap[type];
         ViewModelBase viewModel = null;
         connector.Refrence.TryGetTarget(out viewModel);
         if (viewModel == null)
         {
            viewModel = connector.Getter();
            connector.Refrence.SetTarget(viewModel);
         }
         return viewModel;
      }

      private class ViewModelConnector
      {
         public WeakReference<ViewModelBase> Refrence { get; private set; }
         public Func<ViewModelBase> Getter { get; private set; }

         public ViewModelConnector(Func<ViewModelBase> getter)
         {
            Refrence = new WeakReference<ViewModelBase>(null);
            Getter = getter;
         }
      }
   }
}
