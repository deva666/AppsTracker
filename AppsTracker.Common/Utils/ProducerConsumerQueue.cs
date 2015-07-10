using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppsTracker.Common.Utils
{
    public class ProducerConsumerQueue
    {
        private readonly ConcurrentQueue<Action> queue;
        private readonly AutoResetEvent waitHandle;

        public ProducerConsumerQueue(int workers = 1)
        {
            for (int i = 0; i < workers; i++)
            {
                Task.Factory.StartNew(StartWork, CancellationToken.None, 
                    TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        public void Enqueue(Action work)
        {
            queue.Enqueue(work);
            waitHandle.Set();
        }

        private void StartWork()
        {
            while (true)
            {
                Action work;
                if (queue.TryDequeue(out work))
                {
                    work.Invoke();
                }
                else
                {
                    waitHandle.WaitOne();
                }
            }
        }
    }
}
