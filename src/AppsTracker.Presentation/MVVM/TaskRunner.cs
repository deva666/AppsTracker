using System;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;

namespace AppsTracker.MVVM
{
    public sealed class TaskRunner<T> : AsyncProperty<T>
    {
        private readonly Func<T> valueFactory;

        public TaskRunner(Func<T> valueFactory, IWorker worker)
            : base(worker, null)
        {
            Ensure.NotNull(valueFactory, "valueFactory");

            this.valueFactory = valueFactory;
        }

        protected override Task<T> GetTask()
        {
            return Task<T>.Run(valueFactory);
        }
    }
}
