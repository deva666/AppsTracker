using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Models.Proxy
{
    public interface IAppInfo
    {
        string ProcessName { get; }
        string ProcessVersion { get; }
        string ProcessCompany { get; }
        string ProcessDescription { get; }
        string ProcessFileName { get; }
        string ProcessRealName { get; }
    }
}
