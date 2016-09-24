using System;
using AppsTracker.Views;
using AppsTracker.Widgets;

namespace AppsTracker.Service
{
    public interface IWindowService 
    {
        void ShowMessage(Exception fail);

        void ShowMessage(string message, bool showCancel = true);

        void ShowMessageDialog(Exception fail);

        void ShowMessageDialog(string message, bool showCancel = true);

        void ShowShell(string shellUse);

        IShell GetShell(string shellUse);

        IShell GetMainShell();

        System.Windows.Forms.FolderBrowserDialog CreateFolderBrowserDialog();
        
        void FirstRunWindowSetup();
                
        void OpenMainWindow();
        
        void CloseMainWindow();
        
        void Shutdown();
    }
}
