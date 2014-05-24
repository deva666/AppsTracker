using System;

namespace Task_Logger_Pro.MVVM
{
    /// <summary>
    /// Attribute to decorate a method to be registered to the Mediator
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MediatorMessageSinkAttribute : Attribute
    {
        /// <summary>
        /// The message to register to 
        /// </summary>
		public string Message { get; private set; }

        /// <summary>
        /// The type of parameter for the Method
        /// </summary>
		public Type ParameterType { get; set; }

        /// <summary>
        /// Constructs a method
        /// </summary>
        /// <param name="message">The message to subscribe to</param>
        public MediatorMessageSinkAttribute(string message)
        {
            Message = message;
        }
    }
}