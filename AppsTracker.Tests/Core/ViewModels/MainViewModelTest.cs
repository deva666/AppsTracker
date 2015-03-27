#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using AppsTracker.Data.Service;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Tests.Fakes.Service;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class MainViewModelTest : TestBase
    {
        private bool _refreshCallbackCalled = false;
        private MainViewModel _mainVM;

        [TestInitialize]
        public void Init()
        {
            base.Initialize();

            _mainVM = new MainViewModel();

            Mediator.Instance.Register(MediatorMessages.RefreshLogs, new Action(RefreshLogsCallback));
        }

        private void RefreshLogsCallback()
        {
            _refreshCallbackCalled = true;
        }

        [TestMethod]
        public void TestChildTypes()
        {
            Assert.IsInstanceOfType(_mainVM.UserCollection, typeof(List<Uzer>), "UserCollection types don't match");
            Assert.IsInstanceOfType(_mainVM.SelectedChild, typeof(DataHostViewModel), "Selected child types don't match");
        }

        [TestMethod]
        public void TestChangePageCommand()
        {
            _mainVM.ChangePageCommand.Execute(typeof(StatisticsHostViewModel));
            Assert.IsInstanceOfType(_mainVM.SelectedChild, typeof(StatisticsHostViewModel), "Change page is not working");
        }

        [TestMethod]
        public void TestChangeFirstDate()
        {
            _mainVM.DateFrom = DateTime.Now.AddDays(19);
            Assert.IsTrue(_mainVM.IsFilterApplied, "Date filter not set");
            Assert.IsTrue(_refreshCallbackCalled, "Set first date mediator callback failed");
            _refreshCallbackCalled = false;
        }

        [TestMethod]
        public void TestChangeSecondDate()
        {
            _mainVM.DateTo = DateTime.Now.AddDays(24);
            Assert.IsTrue(_mainVM.IsFilterApplied, "Date filter not set");
            Assert.IsTrue(_refreshCallbackCalled, "Set second date mediator callback failed");
            _refreshCallbackCalled = false;
        }

    }
}
