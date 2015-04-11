using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    class MessageServiceMock : IMessageService
    {
        public void Show(Exception fail)
        {
            
        }

        public void Show(string message, bool showCancel = true)
        {
            
        }

        public void ShowDialog(Exception fail)
        {
            
        }

        public void ShowDialog(string message, bool showCancel = true)
        {
            
        }
    }
}
