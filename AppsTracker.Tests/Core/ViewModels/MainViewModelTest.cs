using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.ViewModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class MainViewModelTest
    {
        [TestInitialize]
        public void Init()
        {
            if (!ServiceFactory.ContainsKey<IAppsService>())
                ServiceFactory.Register<IAppsService>(() => new AppsServiceMock());
        }

        [TestMethod]
        public void TestMainVM()
        {
            var mainVM = new MainViewModel();

            Assert.IsNotNull(mainVM);
            Assert.IsInstanceOfType(mainVM.UserCollection, typeof(List<Uzer>), "UserCollection types don't match");
            Assert.IsInstanceOfType(mainVM.SelectedChild, typeof(DataHostViewModel), "Selected child types don't match");

            mainVM.ChangePageCommand.Execute(typeof(StatisticsHostViewModel));

            Assert.IsInstanceOfType(mainVM.SelectedChild, typeof(StatisticsHostViewModel), "Change page is not working");
        }
    }
}
