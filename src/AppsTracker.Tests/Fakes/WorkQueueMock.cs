using System;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tests.Fakes
{
    public sealed class WorkQueueMock : IWorkQueue
    {
        public Task EnqueueWork(Action work)
        {
            return Task.Run(work);
        }

        public Task<object> EnqueueWork(Func<object> work)
        {
            return Task.Run(work);
        }

        public void Dispose()
        {

        }
    }
}
