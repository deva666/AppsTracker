using System;
using System.Collections.Generic;
using System.Reflection;

namespace AppsTracker.MVVM
{
	/// <summary>
	/// Provides loosely-coupled messaging between
	/// various colleagues.  All references to objects
	/// are stored weakly, to prevent memory leaks.
	/// </summary>
	public class Mediator
	{
        private static readonly Lazy<Mediator> _lazy = new Lazy<Mediator>(() => new Mediator());
        public static Mediator Instance { get { return _lazy.Value; } }

        private Mediator() { }


		readonly MessageToActionsMap invocationList = new MessageToActionsMap();

        /// <summary>
        /// Register a ViewModel to the mediator notifications
        /// This will iterate through all methods of the target passed and will register all methods that are decorated with the MediatorMessageSink Attribute
        /// </summary>
        /// <param name="target">The ViewModel instance to register</param>
		public void Register(object target)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			//Inspect the attributes on all methods and check if there are RegisterMediatorMessageAttribute
			foreach (var methodInfo in target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
				foreach (MediatorMessageSinkAttribute attribute in methodInfo.GetCustomAttributes(typeof(MediatorMessageSinkAttribute), true))
					if (methodInfo.GetParameters().Length == 1)
						invocationList.AddAction(attribute.Message, target, methodInfo, attribute.ParameterType);
					else
						throw new InvalidOperationException("The registered method should only have 1 parameter since the Mediator has only 1 argument to pass");
		}

        /// <summary>
        /// Registers a specific method to the Mediator notifications
        /// </summary>
        /// <param name="message">The message to register to</param>
        /// <param name="callback">The callback function to be called when this message is broadcasted</param>
		public void Register(string message, Delegate callback)
		{
            //This method is not supported in Silverlight
#if SILVERLIGHT
            throw new InvalidOperationException("This method is not supported in Silverlight");
#endif

			if (callback.Target == null)
				throw new InvalidOperationException("Delegate cannot be static");

			ParameterInfo[] parameters = callback.Method.GetParameters();

			// JAS - Changed this logic to allow for 0 or 1 parameter.
			if (parameters != null && parameters.Length > 1)
				throw new InvalidOperationException("The registered delegate should only have 0 or 1 parameter since the Mediator has up to 1 argument to pass");

			// JAS - Pass in the parameter type.
			Type parameterType = (parameters == null || parameters.Length == 0) ? null : parameters[0].ParameterType;
			invocationList.AddAction(message, callback.Target, callback.Method, parameterType);
		}

        /// <summary>
        /// Notify all registered parties that a specific message was broadcasted
        /// </summary>
        /// <typeparam name="T">The Type of parameter to be passed</typeparam>
        /// <param name="message">The message to broadcast</param>
        /// <param name="parameter">The parameter to pass together with the message</param>
		public void NotifyColleagues<T>(string message, T parameter)
		{
			var actions = invocationList.GetActions(message);

			if (actions != null)
				actions.ForEach(action => action.DynamicInvoke(parameter));
		}

		/// <summary>
        /// Notify all registered parties that a specific message was broadcasted
        /// </summary>
        /// <typeparam name="T">The Type of parameter to be passed</typeparam>
        /// <param name="message">The message to broadcast</param>
        public void NotifyColleagues<T>(string message)
		{
			var actions = invocationList.GetActions(message);

			if (actions != null)
				actions.ForEach(action => action.DynamicInvoke());
		}
	}
}