#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class ScreenshotViewModelTest : TestBase
    {
        private volatile bool _loaded = false;

        [TestInitialize]
        public void Init()
        {
            base.Initialize();
        }

        [TestMethod]
        public void TestScreenshotLoader()
        {
            var vm = new ScreenshotsViewModel();
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
