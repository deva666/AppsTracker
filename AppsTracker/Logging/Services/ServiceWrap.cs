using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Logging
{
    internal sealed class ServiceWrap<T> : IDisposable where T : class,IDisposable
    {
        private bool _enabled = false;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (_enabled && _component == null)
                    InitService();
                else if (_enabled == false)
                    DisposeService();
            }
        }

        private T _component;
        public T Component
        {
            get
            {
                if (_component == null)
                    InitService();
                return _component;
            }
        }

        private Func<T> _getter;
        private Action<T> _onInit;
        private Action<T> _onDispose;

        public ServiceWrap(Func<T> getter)
        {
            _getter = getter;
        }

        public ServiceWrap(Func<T> getter, Action<T> onInit, Action<T> onDispose)
            : this(getter)
        {
            _onInit = onInit;
            _onDispose = onDispose;
        }

        public void CallOnService(Action<T> action)
        {
            if (_component != null)
                action(_component);
        }

        public void Dispose()
        {
            DisposeService();
        }

        private void InitService()
        {
            _component = _getter();
            if (_onInit != null)
                _onInit(_component);
        }

        private void DisposeService()
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
