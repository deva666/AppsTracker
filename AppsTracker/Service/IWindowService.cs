using System;
namespace AppsTracker.Service
{
    public interface IWindowService : IBaseService
    {
        void Show(Exception fail);
        void Show(string message, bool showCancel = true);
        void ShowDialog(Exception fail);
        void ShowDialog(string message, bool showCancel = true);
        void ShowWindow<T>() where T : System.Windows.Window;
        void ShowWindow<T>(params object[] args) where T : System.Windows.Window;
        bool? ShowDialog<T>() where T : System.Windows.Window;
        System.Windows.Forms.FolderBrowserDialog CreateFolderBrowserDialog();
        void FirstRunWindowSetup();
        void InitializeTrayIcon();
        void CreateOrShowMainWindow();
        void CloseMainWindow();
        void Shutdown();
    }
}
