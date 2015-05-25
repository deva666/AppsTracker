using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tests.Fakes.Logging
{
    internal class DisposableMock : IDisposable
    {
        public event EventHandler TestEvent;
        public event EventHandler DisposeTestEvent;

        public void FireEvent()
        {
            TestEvent.InvokeSafely(this, EventArgs.Empty);
        }
        public void Dispose()
        {
            DisposeTestEvent.InvokeSafely(this, EventArgs.Empty);
        }
    }
}
