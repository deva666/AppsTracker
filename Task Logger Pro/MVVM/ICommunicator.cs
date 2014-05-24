using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.MVVM
{
  public  interface ICommunicator
    {
      Mediator Mediator { get; }
    }
}
