using System;
namespace AppsTracker.Service
{
    interface IMessageService : IBaseService
    {
        void Show(Exception fail);
        void Show(string message, bool showCancel = true);
        void ShowDialog(Exception fail);
        void ShowDialog(string message, bool showCancel = true);
    }
}
