using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppsTracker.Common.Utils
{
    [Export(typeof(IWorkQueue))]
    public class ProducerConsumerQueue : IWorkQueue
    {
        private bool isDisposed;

        private readonly BlockingCollection<Action> queue;

        public ProducerConsumerQueue()
        {
            queue = new BlockingCollection<Action>();
            Task.Factory.StartNew(StartWork, CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void EnqueueWork(Action work)
        {
            if (queue.IsAddingCompleted)
            {
                Debug.WriteLine("Tried to add work item when queue is completed");
                return;
            }
            queue.Add(work);
        }

        private void StartWork()
        {
            foreach (var work in queue.GetConsumingEnumerable())
            {
                work.Invoke();
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            
            var timeout = TimeSpan.FromMilliseconds(5 * 1000);
            Action work;
            while (queue.TryTake(out work, timeout))
            {
                work.Invoke();
            }
            
            queue.CompleteAdding();
            queue.Dispose();
            isDisposed = true;
        }
    }
}
