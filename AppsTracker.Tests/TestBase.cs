using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Service;
using AppsTracker.Tests.Fakes.Service;

namespace AppsTracker.Tests
{
    public abstract class TestBase
    {
        protected void Initialize()
        {
            if (!ServiceFactory.ContainsKey<IDataService>())
                ServiceFactory.Register<IDataService>(() => new AppsServiceMock());
            if (!ServiceFactory.ContainsKey<IChartService>())
                ServiceFactory.Register<IChartService>(() => new ChartServiceMock());
            if (!ServiceFactory.ContainsKey<ISqlSettingsService>())
                ServiceFactory.Register<ISqlSettingsService>(() => new SqlSettingsServiceMock());
            if (!ServiceFactory.ContainsKey<IXmlSettingsService>())
                ServiceFactory.Register<IXmlSettingsService>(() => new XmlSettingsServiceMock());
        }
    }
}
