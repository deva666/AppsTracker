#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows.Input;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class AboutWindowViewModel : ViewModelBase
    {
        private readonly ILogger logger;


        public override string Title
        {
            get { return "About"; }
        }

        public Version AppVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }


        public string AppName
        {
            get { return Constants.APP_NAME; }
        }


        private ICommand showWebCommand;

        public ICommand ShowWebCommand
        {
            get { return showWebCommand ?? (showWebCommand = new DelegateCommand(ShowWeb)); }
        }

        [ImportingConstructor]
        public AboutWindowViewModel(ILogger _logger)
        {
            logger = _logger;
        }

        private void ShowWeb()
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.theappstracker.com");
            }
            catch (Exception ex)
            {

                logger.Log(ex);
            }
        }
    }
}
