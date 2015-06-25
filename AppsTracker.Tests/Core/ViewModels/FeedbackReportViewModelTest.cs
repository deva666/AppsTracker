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

        [TestInitialize]
        public void Initialize()
        {
            feedbackServiceMock = new Mock<IFeedbackReportService>();
            feedbackServiceMock.Setup(f => f.SendFeedback(It.IsAny<Feedback>())).Returns(() => Task.FromResult(true));
        }


        [TestMethod]
        public void TestSendFeedback()
        {
            var viewModel = new FeedbackReportViewModel(feedbackServiceMock.Object);
            viewModel.SendFeedbackCommand.Execute(null);
            feedbackServiceMock.Verify(f => f.SendFeedback(It.IsAny<Feedback>()));
        }
    }
}
