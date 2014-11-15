using System;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;

using AppsTracker.Logging;
using AppsTracker.Models.EntityModels;
using AppsTracker.Fakes;
using AppsTracker.Models.Proxy;
using AppsTracker.Models.Proxy.Fakes;
using AppsTracker.DAL.Service;
using AppsTracker.DAL.Service.Fakes;
using AppsTracker.Tests.Fakes.Service;



namespace AppsTracker.Tests.Core.Logging
{
    [TestClass]
    public class ComponentContainerTest
    {
        [TestInitialize]
        public void Init()
        {
            if (!ServiceFactory.ContainsKey<IAppsService>())
                ServiceFactory.Register<IAppsService>(() => new AppsServiceMock());
        }

        [TestMethod]
        public void TestComponentContainerConstructor()
        {
            ISettings settings = new StubISettings() { LoggingEnabledGet = () => false };

            ComponentContainer container = new ComponentContainer(settings);

            Assert.IsNotNull(container, "Container not constucted");
        }
    }
}
