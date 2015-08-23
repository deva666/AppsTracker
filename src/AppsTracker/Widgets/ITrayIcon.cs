using System;

namespace AppsTracker.Widgets
{
    public interface ITrayIcon : IDisposable
    {
        bool IsVisible { get; set; }
        System.Windows.Forms.ToolStripMenuItem ShowApp { get; }
    }
}
