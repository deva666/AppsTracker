using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.MVVM
{
    public abstract class SchedulerViewModel : ViewModelBase
    {
        protected TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

    }
}
