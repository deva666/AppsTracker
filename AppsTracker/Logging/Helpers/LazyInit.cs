#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;

namespace AppsTracker.Logging
{
    internal sealed class LazyInit<T> : IDisposable where T : class,IDisposable
    {
        private bool enabled = false;
        private readonly object @lock = new object();

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (enabled)
                    LazyInitValue();
                else
                    DisposeComponent();
            }
        }

        private T component;
        public T Component
        {
            get
            {
                return LazyInitValue();
            }
        }

        private Func<T> getter;
        private Action<T> onInit;
        private Action<T> onDispose;

        public LazyInit(Func<T> getter)
        {
            this.getter = getter;
        }

        public LazyInit(Func<T> getter, Action<T> onInit, Action<T> onDispose)
            : this(getter)
        {
            this.onInit = onInit;
            this.onDispose = onDispose;
        }

        public void CallOn(Action<T> action)
        {
            if (component != null)
                action(component);
        }

        public void Dispose()
        {
            DisposeComponent();
        }

        private T LazyInitValue()
        {
            if (component == null)
            {
                lock (@lock)
                {
                    if (component == null)
                    {
                        component = getter();
                        if (onInit != null)
                            onInit(component);
                    }
                }
            }
            return component;
        }

        private void DisposeComponent()
        {
            lock (@lock)
            {
                if (component != null)
                {
                    if (onDispose != null)
                        onDispose(component);
                    component.Dispose();
                    component = null;
                }

            }
        }
    }
}
