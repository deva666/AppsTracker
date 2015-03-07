#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Data.Service;
using AppsTracker.Pages.ViewModels;
using AppsTracker.Tests.Fakes.Service;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class KeystrokeViewModelTest
    {
        private volatile bool _loaded = false;

        [TestInitialize]
        public void Init()
        {
            if (!ServiceFactory.ContainsKey<IDataService>())
                ServiceFactory.Register<IDataService>(() => new AppsServiceMock());
        }

        [TestMethod]
        public void TestKeystrokeVMLoader()
        {
            var vm = new Data_keystrokesViewModel();
            vm.LogList.PropertyChanged += LogList_PropertyChanged;
            var r = vm.LogList.Result;

            while (_loaded == false)
            {
            }

            Assert.IsNotNull(vm.LogList.Result, "Content not loaded");
        }

        void LogList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Result")
                _loaded = true;
        }
    }
}
