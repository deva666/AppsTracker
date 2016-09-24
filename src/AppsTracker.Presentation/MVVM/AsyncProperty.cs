#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;
using AppsTracker.Domain.Model;

namespace AppsTracker.MVVM
{
    public abstract class AsyncProperty<T> : ObservableObject
    {
        protected readonly IWorker worker;

        private readonly Action<T> onCompletion;

        private Exception exception;
        

        public Exception Exception
        {
            get { return exception; }
            set { SetPropertyValue(ref exception, value); }
        }

        protected Task<T> task;

        public Task<T> Task
        {
            get
            {
                return task;
            }
        }

        protected T result;

        public T Result
        {
            get
            {
                if (task == null)
                {
                    task = GetTask();
                    ObserveTask(task);
                }
                return task.Status == TaskStatus.RanToCompletion ? result : default(T);
            }
        }

        public AsyncProperty(IWorker worker, Action<T> onCompletion)
        {
            Ensure.NotNull(worker, "worker");

            this.worker = worker;
            this.onCompletion = onCompletion;
        }

        public void Reset()
        {
            result = default(T);
            PropertyChanging("Result");
        }

        public void Reload()
        {
            task = GetTask();
            ObserveTask(task);
        }

        protected abstract Task<T> GetTask();

        protected async void ObserveTask(Task<T> task)
        {
            try
            {
                worker.Working = true;
                result = await task.ConfigureAwait(false);
            }
            catch (Exception fail)
            {
                Exception = fail;
            }
            finally
            {
                worker.Working = false;
            }
            if (task.Status == TaskStatus.RanToCompletion)
            {
                PropertyChanging("Result");
                onCompletion?.Invoke(result);
            }
        }
    }
}
