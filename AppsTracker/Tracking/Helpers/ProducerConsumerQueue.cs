using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppsTracker.Tracking.Helpers
{
    internal sealed class ProducerConsumerQueue
    {
        private readonly BlockingCollection<Action> queue;

        public ProducerConsumerQueue()
        {
            queue = new BlockingCollection<Action>();
            Task.Factory.StartNew(new Action(Consume), TaskCreationOptions.LongRunning);
        }

        public void Enqueue(Action work)
        {
            queue.Add(work);
        }

        private void Consume()
        {
            foreach (var work in queue.GetConsumingEnumerable())
            {
                work.Invoke();
            }
        }

        private class WorkItem
        {
            public Action Delegate { get; set; }
        }
    }
}
