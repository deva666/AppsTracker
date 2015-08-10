using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AppsTracker.ViewModels;
using AppsTracker.Views;
using AppsTracker.Widgets;

namespace AppsTracker
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Main window")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MainWindow : Window, IShell
    {
        [ImportingConstructor]
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            this.DataContext = mainViewModel;
            this.Loaded += (s, e) =>
            {
                this.MaxHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            };
        }


        private void Window_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }


        private void MaximizeButton_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            WindowState = System.Windows.WindowState.Maximized;
        }

        private void MinimizeButton_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void ChangeViewButton_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            WindowState = System.Windows.WindowState.Normal;
        }


        public object ViewArgument
        {
            get;
            set;
        }
    }
}
