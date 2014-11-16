#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;

using AppsTracker.MVVM;
using AppsTracker.Tests.Fakes.MVVM;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.MVVM
{
    [TestClass]
    public class MediatorTest
    {
        private bool _typesMatch = false;
        private bool _messageMatch = false;

        [TestInitialize]
        public void Init()
        {
            Mediator.Instance.Register("Test", new Action<object>(MediatorCallback));
        }

        [TestMethod]
        public void TestMediatorNotification()
        {
            Mediator.Instance.NotifyColleagues<MediatorParamMock>("Test", new MediatorParamMock() { Message = "Success" });
            Assert.IsTrue(_typesMatch, "Mediator callback parameter type mismatch");
            Assert.IsTrue(_messageMatch, "Mediator callback parameter message mismatch");
        }

        private void MediatorCallback(object param)
        {
            if (param.GetType() == typeof(MediatorParamMock))
                _typesMatch = true;
            if (((MediatorParamMock)param).Message == "Success")
                _messageMatch = true;
        }

    }
}
