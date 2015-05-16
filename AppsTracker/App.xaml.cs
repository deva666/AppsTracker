#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion


using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AppsTracker.Controllers;
using AppsTracker.Service;

[assembly: InternalsVisibleTo("AppsTracker.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

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

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement)
                                                                , new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline)
                                                                , new PropertyMetadata() { DefaultValue = 40 });

            var context = new DispatcherSynchronizationContext(Application.Current.Dispatcher);
            SynchronizationContext.SetSynchronizationContext(context);

            container = GetCompositionContainer();
            ServiceLocation.ServiceLocator.Instance.Initialize(container);

            bool autostart = args.Where(a => a.ToUpper().Contains(Constants.CMD_ARGS_AUTOSTART)).Count() > 0;
            applicationController = container.GetExportedValue<IApplicationController>();
            applicationController.Initialize(autostart);

            this.SessionEnding += (s, e) => ShutdownApp();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }


        private CompositionContainer GetCompositionContainer()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);
            return container;
        }


        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var fail = (Exception)e.ExceptionObject;
                container.GetExportedValue<IWindowService>().ShowMessageDialog(fail);
                container.GetExportedValue<ILogger>().Log(fail);
            }
            finally
            {
                ShutdownApp();
            }
        }


        internal void ShutdownApp()
        {
            container.Dispose();
            applicationController.ShutDown();
            Application.Current.Shutdown();
        }

        public App() { }
    }
}
