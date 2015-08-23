#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;

namespace AppsTracker.MVVM
{
    public sealed class AsyncProperty<T> : ObservableObject
    {
        private readonly IWorker worker;
        private readonly Func<T> getter;

        private Exception exception;

        public Exception Exception
        {
            get { return exception; }
            set { SetPropertyValue(ref exception, value); }
        }

        private Task<T> task;

        public Task<T> Task
        {
            get { return task; }
            private set { task = value; }
        }

        private T result;

        public T Result
        {
            get
            {
                if (task == null)
                    ScheduleWork();
                return task.Status == TaskStatus.RanToCompletion ? result : default(T);
            }
            private set
            {
                SetPropertyValue(ref result, value);
            }
        }

        public AsyncProperty(Func<T> getter, IWorker worker)
        {
            Ensure.NotNull(getter, "getter");
            Ensure.NotNull(worker, "worker");

            this.worker = worker;
            this.getter = getter;
        }

        public AsyncProperty(Task<T> future, IWorker worker)
        {
            Ensure.NotNull(future, "future");
            Ensure.NotNull(worker, "worker");

            task = future;
            this.worker = worker;

            ObserveTask(task);
        }

        public void Reset()
        {
            Result = default(T);
        }

        public void Reload()
        {
            ScheduleWork();
        }

        private void ScheduleWork()
        {
            task = Task<T>.Run(getter);
            ObserveTask(task);
        }

        private async void ObserveTask(Task<T> task)
        {
            try
            {
                worker.Working = true;
                result = await task.ConfigureAwait(false);
            }
            catch(Exception fail)
            {
                this.Exception = fail;
            }
            finally
            {
                worker.Working = false;
            }
            if (task.Status == TaskStatus.RanToCompletion)
            {
                PropertyChanging("Result");
            }
        }
    }
}
