#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
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
        private readonly Dictionary<Type, ViewModelResolver> childrenMap = new Dictionary<Type, ViewModelResolver>();

        private ViewModelBase selectedChild;

        private ICommand changePageCommand;


        public ViewModelBase SelectedChild
        {
            get { return selectedChild; }
            set
            {
                if (selectedChild != null && selectedChild.Title == value.Title)
                    return;

                SetPropertyValue(ref selectedChild, value);
            }
        }


        public virtual ICommand ChangePageCommand
        {
            get
            {
                return changePageCommand ?? (changePageCommand = new DelegateCommand(ChangePage));
            }
        }


        protected virtual void ChangePage(object parameter)
        {
            SelectedChild = GetChild((Type)parameter);
        }


        protected void RegisterChild<T>(Func<T> valueFactory) where T : ViewModelBase
        {
            if (childrenMap.ContainsKey(typeof(T)))
                return;
            Ensure.NotNull(valueFactory);

            var resolver = new ViewModelResolver(valueFactory);
            childrenMap.Add(typeof(T), resolver);
        }


        protected ViewModelBase GetChild<T>() where T : ViewModelBase
        {
            Ensure.Condition<InvalidOperationException>(childrenMap.ContainsKey(typeof(T)),
                string.Format("Type {0} not registed", typeof(T)));

            var resolver = childrenMap[typeof(T)];
            ViewModelBase viewModel = null;
            resolver.Reference.TryGetTarget(out viewModel);
            if(viewModel == null)
            {
                viewModel = resolver.ValueFactory();
                resolver.Reference.SetTarget(viewModel);
            }
            return viewModel;
        }


        protected ViewModelBase GetChild(Type type)
        {
            Ensure.NotNull(type);
            Ensure.Condition<InvalidOperationException>(childrenMap.ContainsKey(type) == true, string.Format("Can't resolve {0} type!", type));

            var resolver = childrenMap[type];
            ViewModelBase viewModel = null;
            resolver.Reference.TryGetTarget(out viewModel);
            if (viewModel == null)
            {
                viewModel = resolver.ValueFactory();
                resolver.Reference.SetTarget(viewModel);
            }
            return viewModel;
        }


        private class ViewModelResolver
        {
            public WeakReference<ViewModelBase> Reference { get; private set; }
            public Func<ViewModelBase> ValueFactory { get; private set; }

            public ViewModelResolver(Func<ViewModelBase> valueFactory)
            {
                Reference = new WeakReference<ViewModelBase>(null);
                ValueFactory = valueFactory;
            }
        }
    }
}
