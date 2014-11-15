using System;

namespace AppsTracker.Common.Utils
{
    public static class Ensure
    {
        public static void NotNull(object argument)
        {
            if (argument == null)
                throw new ArgumentNullException("Argument is null");
        }

        public static void NotNull(object argument, string argumentName)
        {
            if (argument == null)
                throw new ArgumentNullException(string.Format("{0} is null" , argumentName));
        }

        public static void Condition<TEx>(bool predicate) where TEx : Exception, new()
        {
            if (predicate == false)
                throw new TEx();
        }

        public static void Condition<TEx>(bool predicate, string message) where TEx : Exception
        {
            if (predicate == false)
            {
                TEx ex = (TEx)Activator.CreateInstance(typeof(TEx), message);
                throw ex;
            }
        }

        public static void Condition<TEx>(Func<bool> predicate) where TEx : Exception, new()
        {
            if (predicate() == false)
                throw new TEx();                
        }

        public static void Condition<TEx>(Func<bool> predicate, string message) where TEx : Exception
        {
            if (predicate() == false)
            {
                TEx ex = (TEx)Activator.CreateInstance(typeof(TEx), message);
                throw ex;
            }
        }
    }
}
