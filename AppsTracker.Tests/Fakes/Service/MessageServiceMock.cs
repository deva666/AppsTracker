using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    class MessageServiceMock : IWindowService
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


        public void ShowWindow<T>() where T : System.Windows.Window
        {
            
        }

        public void ShowWindow<T>(params object[] args) where T : System.Windows.Window
        {
            
        }


        public void FirstRunWindowSetup()
        {
            
        }

        public void InitializeTrayIcon()
        {
            
        }

        public void CreateOrShowMainWindow()
        {
            
        }

        public void CloseMainWindow()
        {
            
        }

        public void Shutdown()
        {
            
        }


        public bool? ShowDialog<T>() where T : System.Windows.Window
        {
            return true;
        }

        public System.Windows.Forms.FolderBrowserDialog CreateFolderBrowserDialog()
        {
            return new System.Windows.Forms.FolderBrowserDialog();
        }
    }
}
