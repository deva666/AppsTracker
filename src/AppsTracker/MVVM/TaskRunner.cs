using System;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;

namespace AppsTracker.MVVM
{
    public sealed class TaskRunner<T> : AsyncProperty<T>
    {
        private readonly Func<T> valueFactory;

        public TaskRunner(Func<T> valueFactory, IWorker worker)
            : base(worker)
        {
            Ensure.NotNull(valueFactory, "valueFactory");

            this.valueFactory = valueFactory;
        }

        protected override void ScheduleWork()
        {
            task = Task<T>.Run(valueFactory);
            ObserveTask(task);
        }
    }
}
