using System.ComponentModel.Composition;
using System.Windows;
using AppsTracker.Widgets;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Limit toast window")]
    public partial class LimitToastWindow : Window, IShell
    {
        public LimitToastWindow()
        {
            InitializeComponent();
        }
    }
}
