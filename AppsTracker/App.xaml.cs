#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Runtime;
using System.Windows;
using System.Windows.Media.Animation;
using AppsTracker.Controllers;
using AppsTracker.Controls;
using AppsTracker.Data.Service;


namespace AppsTracker
{
    public partial class App : Application
    {
        private IApplicationController applicationController;

        private AggregateCatalog catalog;

        public App(ReadOnlyCollection<string> args)
        {
            ProfileOptimization.SetProfileRoot(Assembly.GetEntryAssembly().Location);
            ProfileOptimization.StartProfile("StartupProfile");

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

            RegisterServices();

            var container = GetCompositionContainer();

            applicationController = container.GetExportedValue<IApplicationController>();
            applicationController.Initialize(autostart);

            this.SessionEnding += (s, e) => FinishAndExit();

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void RegisterServices()
        {
            ServiceFactory.Register<ISqlSettingsService>(() => SqlSettingsService.Instance);
            ServiceFactory.Register<IXmlSettingsService>(() => XmlSettingsService.Instance);
            ServiceFactory.Register<IAppsService>(() => new AppsService());
            ServiceFactory.Register<IChartService>(() => new ChartService());
        }

        private CompositionContainer GetCompositionContainer()
        {
            catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
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
                AppsTracker.Exceptions.FileLogger.Log(e.Exception);
                MessageWindow messageWindow = new MessageWindow("Ooops, this is awkward ... something went wrong." +
                       Environment.NewLine + "The app needs to close." + Environment.NewLine + "Error: " + e.Exception.Message);
                messageWindow.ShowDialog();
            }
            finally
            {
                FinishAndExit();
            }
        }

        internal void FinishAndExit()
        {
            applicationController.ShutDown();
            Application.Current.Shutdown();
        }

        public App() { }
    }
}
