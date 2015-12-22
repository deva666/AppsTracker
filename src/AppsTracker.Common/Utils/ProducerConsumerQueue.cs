using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Common.Logging;

namespace AppsTracker.Common.Utils
{
    [Export(typeof(IWorkQueue))]
    public sealed class ProducerConsumerQueue : IWorkQueue
    {
        private bool isDisposed;

        private readonly ILogger logger;
        private readonly BlockingCollection<WorkItem> queue;

        [ImportingConstructor]
        public ProducerConsumerQueue(ILogger logger)
        {
            this.logger = logger;

            queue = new BlockingCollection<WorkItem>();
            Task.Factory.StartNew(StartWork, CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public Task EnqueueWork(Action work)
        {
            var taskSource = new TaskCompletionSource<Object>();
            queue.Add(new WorkItem(work, taskSource));
            return taskSource.Task;
        }

        public Task<Object> EnqueueWork(Func<Object> work)
        {
            var taskSource = new TaskCompletionSource<Object>();
            queue.Add(new WorkItem(work, taskSource));
            return taskSource.Task;
        }

        private void StartWork()
        {
            foreach (var work in queue.GetConsumingEnumerable())
            {
                try
                {
                    object result = null;
                    if (work.Action != null)
                    {
                        work.Action();
                    }
                    else if (work.ValueFactory != null)
                    {
                        result = work.ValueFactory.Invoke();
                    }
                    work.TaskSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    logger.Log(ex);
                    work.TaskSource.SetException(ex);
                }
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            queue.CompleteAdding();
            queue.WaitUntilEmpty();
            isDisposed = true;
        }

        private struct WorkItem
        {
            public Action Action { get; private set; }
            public Func<Object> ValueFactory { get; private set; }
            public TaskCompletionSource<Object> TaskSource { get; private set; }

            public WorkItem(Action action, TaskCompletionSource<Object> taskSource)
                : this()
            {
                Action = action;
                TaskSource = taskSource;
            }

            public WorkItem(Func<Object> valueFactory, TaskCompletionSource<Object> taskSource)
                : this()
            {
                ValueFactory = valueFactory;
                TaskSource = taskSource;
            }
        }
    }
}
