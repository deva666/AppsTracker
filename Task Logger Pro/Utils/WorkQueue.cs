using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Utils
{
    class WorkQueue<T> 
    {
        private Queue<T> _queue;

        public WorkQueue()
        {
            _queue = new Queue<T>();
        }

        public void Add(T item)
        {
            if (_queue.Count == 0)
            {
                var handler = WorkStarted;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            _queue.Enqueue(item);
        }

        public void Remove()
        {
            _queue.Dequeue();
            if (_queue.Count == 0)
            {
                var handler = WorkEnded;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler WorkStarted;
        public event EventHandler WorkEnded;
    }
}
