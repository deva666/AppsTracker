using System;
using System.Reflection;

namespace AppsTracker.ServiceLocation
{
    internal class WeakAction
    {
        readonly MethodInfo method;
        readonly Type delegateType;
        readonly WeakReference weakRef;

        internal WeakAction(object target, MethodInfo method, Type parameterType)
        {
            weakRef = new WeakReference(target);

            this.method = method;

            if (parameterType == null)
                this.delegateType = typeof(Action);
            else
                this.delegateType = typeof(Action<>).MakeGenericType(parameterType);
        }

        internal Delegate CreateAction()
        {
            object target = weakRef.Target;
            if (target != null)
            {
                return Delegate.CreateDelegate(
                            this.delegateType,
                            weakRef.Target,
                            method);
            }
            else
            {
                return null;
            }
        }

        public bool IsAlive
        {
            get
            {
                return weakRef.IsAlive;
            }
        }
    }
}