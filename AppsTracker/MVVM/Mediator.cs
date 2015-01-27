using System;

namespace AppsTracker.MVVM
{
    /// <summary>
    /// Provides loosely-coupled messaging between
    /// various colleagues.  All references to objects
    /// are stored weakly, to prevent memory leaks.
    /// </summary>
    public class Mediator : IMediator
    {
        private static readonly Lazy<Mediator> _lazy = new Lazy<Mediator>(() => new Mediator());
        public static Mediator Instance { get { return _lazy.Value; } }

        private Mediator() { }

        private readonly MessageToActionsMap _invocationList = new MessageToActionsMap();
        private readonly MessageToActionsMap _parameterInvocationList = new MessageToActionsMap();

        public void Register(string message, Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callBack");

            _invocationList.AddAction(message, callback.Target, callback.Method, null);
        }

        public void Register<T>(string message, Action<T> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callBack");

            _parameterInvocationList.AddAction(message, callback.Target, callback.Method, typeof(T));
        }

        /// <summary>
        /// Notify all registered parties that a specific message was broadcasted
        /// </summary>
        /// <typeparam name="T">The Type of parameter to be passed</typeparam>
        /// <param name="message">The message to broadcast</param>
        /// <param name="parameter">The parameter to pass together with the message</param>
        public void NotifyColleagues<T>(string message, T parameter)
        {
            var actions = _parameterInvocationList.GetActions(message);

            if (actions != null)
                actions.ForEach(action => ((Action<T>)action).Invoke(parameter));
        }

        /// <summary>
        /// Notify all registered parties that a specific message was broadcasted
        /// </summary>
        /// <typeparam name="T">The Type of parameter to be passed</typeparam>
        /// <param name="message">The message to broadcast</param>
        public void NotifyColleagues(string message)
        {
            var actions = _invocationList.GetActions(message);

            if (actions != null)
                actions.ForEach(action => ((Action)action).Invoke());
        }
    }
}