#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.ObjectModel;
using AppsTracker.Views;
using Microsoft.VisualBasic.ApplicationServices;

namespace AppsTracker
{
    public static class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#if DEBUG
            RunApp(new ReadOnlyCollection<string>(args));
#else

            SingleInstanceManager singleInstanceApp = new SingleInstanceManager();
            singleInstanceApp.Run(args);
#endif
        }

        private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            var app = App.Current as App;
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                FileLogger.Instance.Log(ex);

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MessageWindow messageWindow = new MessageWindow(ex);
                    messageWindow.ShowDialog();
                }));

            }
            finally
            {
                if (app != null)
                    app.FinishAndExit();
            }
        }

        private static void RunApp(ReadOnlyCollection<String> eventArgs)
        {
            if (eventArgs.Count == 0)
            {
                System.Windows.SplashScreen splashScreen = new System.Windows.SplashScreen("resources/appstrackersplashresized.png");
                splashScreen.Show(true);
            }
            App app = new App(eventArgs);
            app.Run();
        }

        public class SingleInstanceManager : WindowsFormsApplicationBase
        {
            public static event EventHandler SecondInstanceActivating;

            public SingleInstanceManager()
            {
                this.IsSingleInstance = true;
            }

            protected override bool OnStartup(StartupEventArgs eventArgs)
            {
                RunApp(eventArgs.CommandLine);
                return false;
            }

            protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
            {
                base.OnStartupNextInstance(eventArgs);
                SecondInstanceActivating.InvokeSafely(this, EventArgs.Empty);
            }
        }
    }
}
