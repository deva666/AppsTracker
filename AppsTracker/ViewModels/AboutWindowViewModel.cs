using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.MVVM
{
    internal sealed class AboutWindowViewModel : ViewModelBase
    {
        public override string Title { get { return "About"; } }
        public Version AppVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
        public string AppName { get { return Constants.APP_NAME; } }
        public string License { get { if (App.UzerSetting.Licence) return "Licensed version."; else return "Trial version."; } }        
    }
}
