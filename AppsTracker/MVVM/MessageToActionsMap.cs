using System;
using System.Collections.Generic;
using System.Reflection;

namespace AppsTracker.MVVM
{
    /// <summary>
    /// This class is an implementation detail of the Mediator class.
    /// This will store all actions to be invoked
    /// </summary>
    internal class MessageToActionsMap
    {
        //store a hash where the key is the message and the value is the list of Actions to call
        readonly Dictionary<string, List<WeakAction>> map = new Dictionary<string, List<WeakAction>>();

        /// <summary>
        /// Adds an action to the list
        /// </summary>
        /// <param name="message">The message to register to </param>
        /// <param name="target">The target object to invoke</param>
        /// <param name="method">The method in the target object to invoke</param>
        /// <param name="actionType">The Type of the action</param>
        internal void AddAction(string message, object target, MethodInfo method, Type actionType)		
        {
            if (message == null)
                throw new ArgumentNullException("message");

			if (method == null)
                throw new ArgumentNullException("method");

            lock (map)//lock on the dictionary 
            {
                if (!map.ContainsKey(message))
                    map[message] = new List<WeakAction>();

                map[message].Add(new WeakAction(target, method, actionType));
            }
        }

        /// <summary>
        /// Gets the list of actions to be invoked for the specified message
        /// </summary>
        /// <param name="message">The message to get the actions for</param>
        /// <returns>Returns a list of actions that are registered to the specified message</returns>
        internal List<Delegate> GetActions(string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            List<Delegate> actions;
            lock (map)
            {
                if (!map.ContainsKey(message))
                    return null;

                List<WeakAction> weakActions = map[message];
                actions = new List<Delegate>(weakActions.Count);
                for (int i = weakActions.Count - 1; i > -1; --i)
                {
                    WeakAction weakAction = weakActions[i];
                    if (!weakAction.IsAlive)
                        weakActions.RemoveAt(i);
                    else
                        actions.Add(weakAction.CreateAction());
                }

                //delete the list from the hash if it is now empty
                if (weakActions.Count == 0)
                    map.Remove(message);
            }
            return actions;
        }
    }
}