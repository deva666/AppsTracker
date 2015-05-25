#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using AppsTracker.MVVM;
using AppsTracker.Tests.Fakes.MVVM;
using AppsTracker.Common.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.MVVM
{
    [TestClass]
    public class MediatorTest
    {
        private readonly string _parameterMessage = "Callback with parameter";
        private readonly string _emptyMessage = "Empty callback";
        private readonly Mediator mediator = new Mediator();

        private bool _typesMatch = false;
        private bool _messageMatch = false;
        private bool _callbackTriggered = false;

        [TestInitialize]
        public void Init()
        {
            mediator.Register(_parameterMessage, new Action<object>(ParameterCallback));
            mediator.Register(_emptyMessage, new Action(EmptyCallback));
        }

        [TestMethod]
        public void TestMediatorNotification()
        {
            mediator.NotifyColleagues<MediatorParamMock>(_parameterMessage, new MediatorParamMock() { Message = "Success" });
            mediator.NotifyColleagues(_emptyMessage);
            Assert.IsTrue(_typesMatch, "Mediator callback parameter type mismatch");
            Assert.IsTrue(_messageMatch, "Mediator callback parameter message mismatch");
            Assert.IsTrue(_callbackTriggered, "Mediator callback not triggered");
        }

        private void ParameterCallback(object param)
        {
            if (param.GetType() == typeof(MediatorParamMock))
                _typesMatch = true;
            if (((MediatorParamMock)param).Message == "Success")
                _messageMatch = true;
        }

        private void EmptyCallback()
        {
            _callbackTriggered = true;
        }

    }
}
