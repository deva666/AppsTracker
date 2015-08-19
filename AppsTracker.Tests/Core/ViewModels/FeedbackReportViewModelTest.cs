using System;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Service.Web;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class FeedbackReportViewModelTest
    {
        private Mock<IFeedbackReportService> feedbackServiceMock;
        private Mock<ILogger> loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            feedbackServiceMock = new Mock<IFeedbackReportService>();
            loggerMock = new Mock<ILogger>();
            feedbackServiceMock.Setup(f => f.SendFeedback(It.IsAny<Feedback>()))
                .Returns(() => Task.FromResult(true));
        }


        [TestMethod]
        public void TestSendFeedback()
        {
            var viewModel = new FeedbackReportViewModel(feedbackServiceMock.Object, loggerMock.Object);
            viewModel.SendFeedbackCommand.Execute(null);
            feedbackServiceMock.Verify(f => f.SendFeedback(It.IsAny<Feedback>()));
            Assert.AreEqual(viewModel.InfoMessage, "Feedback sent");
        }
    }
}
