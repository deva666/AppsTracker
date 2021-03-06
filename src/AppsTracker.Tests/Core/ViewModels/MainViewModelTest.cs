﻿#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Settings;
using AppsTracker.Domain.Users;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class MainViewModelTest : TestMockBase
    {
        private MainViewModel mainViewModel;
        private Setting setting = new Setting() { TrackingEnabled = true };


        [TestInitialize]
        public void Init()
        {
            repository.Setup(d => d.GetFiltered<Uzer>(u => true)).Returns(new List<Uzer>());
            trackingService.Setup(t => t.DateFrom).Returns(DateTime.Now.AddDays(-10));
            trackingService.Setup(t => t.DateTo).Returns(DateTime.Now);
            settingsService.Setup(s => s.Settings).Returns(setting);
            settingsService.Setup(s => s.SaveChanges(It.IsAny<Setting>())).Callback<Setting>(s => setting = s);
            userSettingsService.Setup(x => x.AppSettings).Returns(new AppSettings() { DisableNotifyForNewVersion = false });


            var dataHostVMFactory = GetDataHostVMFactory();
            var statisticsHostVMFactory = GetStatisticsHostVMFactory();
            var settingsHostVMFactory = GetSettingsHostVMFactory();

            mainViewModel = new MainViewModel(repository.Object,
                settingsService.Object,
                userSettingsService.Object,
                trackingService.Object,
                releaseNotesService.Object,
                windowService.Object,
                mediator,
                dataHostVMFactory,
                statisticsHostVMFactory,
                settingsHostVMFactory);
        }

        private MainViewModel CreateViewModel()
        {
            var dataHostVMFactory = GetDataHostVMFactory();
            var statisticsHostVMFactory = GetStatisticsHostVMFactory();
            var settingsHostVMFactory = GetSettingsHostVMFactory();

            return new MainViewModel(repository.Object,
                settingsService.Object,
                userSettingsService.Object,
                trackingService.Object,
                releaseNotesService.Object,
                windowService.Object,
                mediator,
                dataHostVMFactory,
                statisticsHostVMFactory,
                settingsHostVMFactory);
        }

        [TestMethod]
        public async Task TestFailedVersionGet()
        {
            releaseNotesService.Setup(s => s.GetReleaseNotesAsync())
                .ThrowsAsync(new Exception("Fail"));
            var viewModel = CreateViewModel();
            await Task.Delay(200);
            Assert.IsFalse(viewModel.NewVersionAvailable);
        }

        [TestMethod]
        public async Task TestNoNewVersion()
        {
            releaseNotesService.Setup(s => s.GetReleaseNotesAsync())
                .ReturnsAsync(new List<ReleaseNote>() { new ReleaseNote("0.0.0.1", new List<String>()) });
            var viewModel = CreateViewModel();
            await Task.Delay(200);
            Assert.IsFalse(viewModel.NewVersionAvailable);
        }

        [TestMethod]
        public async Task TestNewVersionAvailable()
        {
            releaseNotesService.Setup(s => s.GetReleaseNotesAsync())
                .ReturnsAsync(new List<ReleaseNote>() { new ReleaseNote("99.99.99.99", new List<String>()) });
            var viewModel = CreateViewModel();
            await Task.Delay(200);
            Assert.IsTrue(viewModel.NewVersionAvailable);
        }

        [TestMethod]
        public void TestChildTypes()
        {
            Assert.IsInstanceOfType(mainViewModel.UserCollection, typeof(IEnumerable<UserModel>), "UserCollection types don't match");
            Assert.IsInstanceOfType(mainViewModel.SelectedChild, typeof(DataHostViewModel), "Selected child types don't match");
        }

        [TestMethod]
        public void TestChangeFirstDate()
        {
            mainViewModel.DateFrom = DateTime.Now.AddDays(19);
            Assert.IsTrue(mainViewModel.IsFilterApplied, "Date filter not set");
            trackingService.VerifySet(t => t.DateFrom = It.IsAny<DateTime>(), "Date should also be set on the tracking service");
            mainViewModel.ClearFilterCommand.Execute(null);
            Assert.IsFalse(mainViewModel.IsFilterApplied, "Date filter should be false after ClearFilterCommand execution");
            trackingService.VerifySet(t => t.DateFrom = It.IsAny<DateTime>(), "Date should also be set on the tracking service");
        }

        [TestMethod]
        public void TestChangeSecondDate()
        {
            mainViewModel.DateTo = DateTime.Now.AddDays(24);
            Assert.IsTrue(mainViewModel.IsFilterApplied, "Date filter not set");
            trackingService.VerifySet(t => t.DateTo = It.IsAny<DateTime>(), "Date should also be set on the tracking service");
            mainViewModel.ClearFilterCommand.Execute(null);
            Assert.IsFalse(mainViewModel.IsFilterApplied, "Date filter should be false after ClearFilterCommand execution");
            trackingService.VerifySet(t => t.DateTo = It.IsAny<DateTime>(), "Date should also be set on the tracking service");
        }

        [TestMethod]
        public void TestSettingsNavigation()
        {
            var selectedChild = mainViewModel.SelectedChild;
            mainViewModel.GoToSettingsCommand.Execute(null);
            Assert.IsInstanceOfType(selectedChild, mainViewModel.ToSettings, "Navigating to settings should set ToSettings type to type tha navigation came from");
            mainViewModel.ReturnFromSettingsCommand.Execute(null);
            Assert.AreSame(selectedChild, mainViewModel.SelectedChild, "Returning from settings should set SelectedChild to viewModel navigation came from");
        }

        [TestMethod]
        public void TestChangeTrackingStatus()
        {
            var trackingStatus = mainViewModel.UserSettings.TrackingEnabled;
            mainViewModel.ChangeLoggingStatusCommand.Execute(null);
            Assert.AreNotEqual(trackingStatus, mainViewModel.UserSettings.TrackingEnabled);
            settingsService.Verify(s => s.SaveChanges(It.IsAny<Setting>()));
        }
    }
}
