using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppsTracker.Utils
{
    class WorkQueue<T> 
    {
        private ConcurrentQueue<T> _queue;

        public WorkQueue()
        {
            _queue = new ConcurrentQueue<T>();
        }

        public void Add(T item)
        {
            if (_queue.Count == 0)
            {
                var handler = Volatile.Read(ref WorkStarted);
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            _queue.Enqueue(item);
        }

        public T Remove()
        {
            T item;
            _queue.TryDequeue(out item);
            if (_queue.Count == 0)
            {
                var handler = Volatile.Read(ref WorkEnded);
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            return item;
        }

        public event EventHandler WorkStarted;
        public event EventHandler WorkEnded;
    }
}
