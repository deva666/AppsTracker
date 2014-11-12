using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppsTracker.MVVM
{
    internal class AsyncProperty<T> : ObservableObject
    {
        private ViewModelBase _host;
        private Func<T> _getter;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        
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

        public AsyncProperty(Func<T> getter, ViewModelBase host)
        {
            _host = host;
            _getter = getter;
        }

        public void Reset()
        {
            Result = default(T);
        }

        public void Reload()
        {
            if (_task != null && _task.Status == TaskStatus.Running)
                _tokenSource.Cancel();            
            ScheduleWork();
        }

        private void ScheduleWork()
        {
            _task = Task<T>.Factory.StartNew(_getter, _tokenSource.Token);
            ObserveTask(_task);
        }

        private async void ObserveTask(Task<T> task)
        {
            try
            {
                _host.Working = true;
                _result = await task.ConfigureAwait(false);                
            }
            catch
            {
            }
            finally
            {
                _host.Working = false;
            }
            if (task.Status == TaskStatus.RanToCompletion)
            {
                PropertyChanging("Result");
            }
        }

    }
}
