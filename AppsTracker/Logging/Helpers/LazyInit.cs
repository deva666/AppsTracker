using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppsTracker.Logging
{
    internal sealed class LazyInit<T> : IDisposable where T : class,IDisposable
    {
        private bool _enabled = false;
        private object @lock = new object();

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (_enabled)
                    LazyInitValue();
                else
                    DisposeComponent();
            }
        }

        private T _component;
        public T Component
        {
            get
            {
                return LazyInitValue();
            }
        }

        private Func<T> _getter;
        private Action<T> _onInit;
        private Action<T> _onDispose;

        public LazyInit(Func<T> getter)
        {
            _getter = getter;
        }

        public LazyInit(Func<T> getter, Action<T> onInit, Action<T> onDispose)
            : this(getter)
        {
            _onInit = onInit;
            _onDispose = onDispose;
        }

        public void CallOn(Action<T> action)
        {
            if (_component != null)
                action(_component);
        }

        public void Dispose()
        {
            DisposeComponent();
        }

        private T LazyInitValue()
        {
            if (_component == null)
            {
                lock (@lock)
                {
                    if (_component == null)
                    {
                        _component = _getter();
                        if (_onInit != null)
                            _onInit(_component);
                    }
                }
            }
            return _component;
        }

        private void DisposeComponent()
        {
            if (_component != null)
            {
                lock (@lock)
                {
                    if (_component != null)
                    {
                        if (_onDispose != null)
                            _onDispose(_component);
                        _component.Dispose();
                        _component = null;
                    }
                }
            }
        }
    }
}
