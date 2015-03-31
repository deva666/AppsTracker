using System;
using System.Collections.Generic;
using System.Reflection;

namespace AppsTracker.ServiceLocation
{
    internal class MessageToActionsMap
    {
        readonly Dictionary<string, List<WeakAction>> map = new Dictionary<string, List<WeakAction>>();

        internal void AddAction(string message, object target, MethodInfo method, Type actionType)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (method == null)
                throw new ArgumentNullException("method");

            lock (map)
            {
                if (!map.ContainsKey(message))
                    map[message] = new List<WeakAction>();

                map[message].Add(new WeakAction(target, method, actionType));
            }
        }

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

                if (weakActions.Count == 0)
                    map.Remove(message);
            }
            return actions;
        }
    }
}