#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppsTracker.MVVM
{
    internal class AsyncProperty<T> : ObservableObject
    {
        private IWorker _worker;
        private Func<T> _getter;

        private Task<T> _task;

        public Task<T> Task
        {
            get { return _task; }
            private set { _task = value; }
        }

        private T _result;

        public T Result
        {
            get
            {
                if (_task == null)
                    ScheduleWork();
                return _task.Status == TaskStatus.RanToCompletion ? _result : default(T);
            }
            private set
            {
                _result = value;
                PropertyChanging("Result");
            }
        }

        public AsyncProperty(Func<T> getter, IWorker worker)
        {
            _worker = worker;
            _getter = getter;
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
            _task = Task<T>.Run(_getter);
            ObserveTask(_task);
        }

        private async void ObserveTask(Task<T> task)
        {
            try
            {
                _worker.Working = true;
                _result = await task.ConfigureAwait(false);
            }
            catch
            {
            }
            finally
            {
                _worker.Working = false;
            }
            if (task.Status == TaskStatus.RanToCompletion)
            {
                PropertyChanging("Result");
            }
        }

    }
}
