using System;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;

namespace AppsTracker.MVVM
{
    public sealed class TaskObserver<T> : AsyncProperty<T>
    {
        private readonly Func<Task<T>> valueFactory;

        public TaskObserver(Func<Task<T>> valueFactory, IWorker worker)
            : base(worker)
        {
            Ensure.NotNull(valueFactory, "valueFactory");

            this.valueFactory = valueFactory;
        }

        protected override void ScheduleWork()
        {
            task = valueFactory();
            ObserveTask(task);
        }
    }
}
