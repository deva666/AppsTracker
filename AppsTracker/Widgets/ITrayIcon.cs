using System;

namespace AppsTracker.Widgets
{
    interface ITrayIcon : IDisposable
    {
        bool IsVisible { get; set; }
        System.Windows.Forms.ToolStripMenuItem ShowApp { get; }
    }
}
