using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.MVVM
{
    class AboutWindowViewModel : ViewModelBase
    {
        public Version AppVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
        public string AppName { get { return Constants.APP_NAME; } }
        public string License { get { if (  App.UzerSetting.Licence ) return "Licensed version."; else return "Trial version."; } }
    }
}
