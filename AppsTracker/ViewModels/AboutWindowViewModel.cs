#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Reflection;

namespace AppsTracker.MVVM
{
    internal sealed class AboutWindowViewModel : ViewModelBase
    {
        public override string Title { get { return "About"; } }

        public Version AppVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        public string AppName { get { return Constants.APP_NAME; } }
    }
}
