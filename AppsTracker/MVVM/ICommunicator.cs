using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.MVVM
{
    public interface ICommunicator
    {
        IMediator Mediator { get; }
    }
}
