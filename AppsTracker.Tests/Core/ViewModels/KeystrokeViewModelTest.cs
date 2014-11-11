using System;
using System.Collections.Generic;

using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.MVVM;
using AppsTracker.Pages.ViewModels;

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
            if (!ServiceFactory.ContainsKey<IAppsService>())
                ServiceFactory.Register<IAppsService>(() => new AppsServiceMock());
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
