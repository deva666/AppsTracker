#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class MainViewModelTest : TestMockBase
    {
        private MainViewModel mainViewModel;

        [TestInitialize]
        public void Init()
        {
            dataService.Setup(d => d.GetFiltered<Uzer>(u => true)).Returns(new List<Uzer>());

            var dataHostVMFactory = GetDataHostVMFactory();
            var statisticsHostVMFactory = GetStatisticsHostVMFactory();
            var settingsHostVMFactory = GetSettingsHostVMFactory();

            mainViewModel = new MainViewModel(dataService.Object,
                settingsService.Object,
                trackingService.Object,
                mediator.Object,
                dataHostVMFactory,
                statisticsHostVMFactory,
                settingsHostVMFactory);                
        }

        [TestMethod]
        public void TestChildTypes()
        {
            Assert.IsInstanceOfType(mainViewModel.UserCollection, typeof(List<Uzer>), "UserCollection types don't match");
            Assert.IsInstanceOfType(mainViewModel.SelectedChild, typeof(DataHostViewModel), "Selected child types don't match");
        }

        [TestMethod]
        public void TestChangePageCommand()
        {
            mainViewModel.ChangePageCommand.Execute(typeof(StatisticsHostViewModel));
            Assert.IsInstanceOfType(mainViewModel.SelectedChild, typeof(StatisticsHostViewModel), "Change page is not working");
        }

        [TestMethod]
        public void TestChangeFirstDate()
        {
            mainViewModel.DateFrom = DateTime.Now.AddDays(19);
            Assert.IsTrue(mainViewModel.IsFilterApplied, "Date filter not set");
            mainViewModel.ClearFilterCommand.Execute(null);
            Assert.IsFalse(mainViewModel.IsFilterApplied, "Date filter should be false after ClearFilterCommand execution");
        }

        [TestMethod]
        public void TestChangeSecondDate()
        {
            mainViewModel.DateTo = DateTime.Now.AddDays(24);
            Assert.IsTrue(mainViewModel.IsFilterApplied, "Date filter not set");
            mainViewModel.ClearFilterCommand.Execute(null);
            Assert.IsFalse(mainViewModel.IsFilterApplied, "Date filter should be false after ClearFilterCommand execution");
        }

        [TestMethod]
        public void TestSettingsNavigation()
        {
            var selectedChild = mainViewModel.SelectedChild;
            mainViewModel.GoToSettingsCommand.Execute(null);
            Assert.IsInstanceOfType(selectedChild, mainViewModel.ToSettings, "Navigating to settings should set ToSettings type to type tha navigation came from");
        }
    }
}
