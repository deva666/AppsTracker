using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace AppsTracker.Common.Utils
{
    [Export(typeof(IWorkQueue<>))]
    public sealed class ProducerConsumerQueue<T> : IWorkQueue<T>
    {
        private bool isDisposed;

        private readonly BlockingCollection<WorkItem<T>> queue;

        public ProducerConsumerQueue()
        {
            queue = new BlockingCollection<WorkItem<T>>();
            Task.Factory.StartNew(StartWork, CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }


        public Task<T> EnqueueWork(Func<T> work)
        {
            var taskSource = new TaskCompletionSource<T>();
            queue.Add(new WorkItem<T>(work, taskSource));
            return taskSource.Task;
        }

        private void StartWork()
        {
            foreach (var work in queue.GetConsumingEnumerable())
            {
                try
                {
                    var result = work.ValueFactory.Invoke();
                    work.TaskSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    work.TaskSource.SetException(ex);
                }
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            queue.CompleteAdding();
            isDisposed = true;
        }

        private class WorkItem<T>
        {
            public Func<T> ValueFactory { get; set; }
            public TaskCompletionSource<T> TaskSource { get; set; }


            public WorkItem(Func<T> valueFactory, TaskCompletionSource<T> taskSource)
            {
                ValueFactory = valueFactory;
                TaskSource = taskSource;
            }
        }
    }
}

