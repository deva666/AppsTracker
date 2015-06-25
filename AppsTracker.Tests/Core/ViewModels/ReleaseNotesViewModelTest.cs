using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Service.Web;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class ReleaseNotesViewModelTest
    {
        private readonly Mock<IReleaseNotesService> releaseNotesServiceMock 
            = new Mock<IReleaseNotesService>();

        [TestInitialize]
        public void Initialize()
        {
            releaseNotesServiceMock.Setup(r => r.GetReleaseNotesAsync())
                .ReturnsAsync(new List<ReleaseNote>());
        }

        [TestMethod]
        public void TestGetReleaseNotes()
        {
            var viewModel = new ReleaseNotesViewModel(releaseNotesServiceMock.Object);
            var result = viewModel.ReleaseNotes.Result;
            while (viewModel.Working)
            {
                
            }
            releaseNotesServiceMock.Verify(r => r.GetReleaseNotesAsync());
            Assert.IsInstanceOfType(viewModel.ReleaseNotes.Result, typeof(IEnumerable<ReleaseNote>));
        }
    }
}
