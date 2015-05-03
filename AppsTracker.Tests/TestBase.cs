using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using AppsTracker.Tracking.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        private CompositionContainer container;
        private AggregateCatalog catalog;

        public CompositionContainer Container { get { return container; } }

        [TestInitialize]
        protected void Initialize()
        {
            catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(TestBase).Assembly));
            container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);

            ServiceLocation.ServiceLocator.Instance.Initialize(container);

            var syncContext = container.GetExportedValue<ISyncContext>();
            syncContext.Context = new System.Threading.SynchronizationContext();
        }
    }
}
