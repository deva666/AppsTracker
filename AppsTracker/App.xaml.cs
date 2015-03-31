#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Runtime;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AppsTracker.Controllers;
using AppsTracker.Data.Service;
using AppsTracker.Views;


namespace AppsTracker
{
    public partial class App : Application
    {
        private readonly IApplicationController applicationController;
        private readonly CompositionContainer container;

        public App(ReadOnlyCollection<string> args)
        {
            ProfileOptimization.SetProfileRoot(Assembly.GetEntryAssembly().Location);
            ProfileOptimization.StartProfile("AppsTrackerProfile");

            InitializeComponent();

            this.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

            bool autostart = false;
            foreach (var arg in args)
                if (arg.ToUpper().Contains(Constants.CMD_ARGS_AUTOSTART))
                    autostart = true;

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement)
                                                                , new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline)
                                                                , new PropertyMetadata() { DefaultValue = 40 });

            var context = new DispatcherSynchronizationContext(Application.Current.Dispatcher);
            SynchronizationContext.SetSynchronizationContext(context);

            container = GetCompositionContainer();
            AppsTracker.ServiceLocation.ServiceLocator.Instance.Initialize(container);

            applicationController = container.GetExportedValue<IApplicationController>();
            applicationController.Initialize(autostart);

            this.SessionEnding += (s, e) => FinishAndExit();
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private CompositionContainer GetCompositionContainer()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AppsTracker.Data.Db.AppsEntities).Assembly));
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);
            return container;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                MessageWindow messageWindow = new MessageWindow(e.Exception);
                messageWindow.ShowDialog();
                container.GetExportedValue<ILogger>().Log(e.Exception);
            }
            finally
            {
                FinishAndExit();
            }
        }

        internal void FinishAndExit()
        {
            container.Dispose();
            applicationController.ShutDown();
            Application.Current.Shutdown();
        }

        public App() { }
    }
}
