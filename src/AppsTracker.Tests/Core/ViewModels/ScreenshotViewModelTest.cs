#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AppsTracker.Data.Models;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class ScreenshotViewModelTest : TestMockBase
    {
        [TestInitialize]
        public void Init()
        {
            dataService.Setup(d => d.GetFilteredAsync<Log>(It.IsAny<Expression<Func<Log, bool>>>(),
                 It.IsAny<Expression<Func<Log, object>>>(),
                 It.IsAny<Expression<Func<Log, object>>>()))
                .ReturnsAsync(new List<Log>());
        }

        [TestMethod]
        public void TestGetLogs()
        {
            var viewModel = new ScreenshotsViewModel(dataService.Object,
                                                     settingsService.Object,
                                                     trackingService.Object,
                                                     windowService.Object,
                                                     mediator);


            var result = viewModel.LogList.Result;
            while (viewModel.Working)
            {
            }

            Assert.IsInstanceOfType(viewModel.LogList.Result, typeof(IEnumerable<Log>));
            dataService.Verify(d => d.GetFilteredAsync<Log>(It.IsAny<Expression<Func<Log, bool>>>(),
                 It.IsAny<Expression<Func<Log, object>>>(),
                 It.IsAny<Expression<Func<Log, object>>>()));
        }
    }
}
