using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Controllers
{
    public interface IApplicationController
    {
        void Initialize(bool autoStart);
        void ShutDown();
    }
}
