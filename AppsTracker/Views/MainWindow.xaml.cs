using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AppsTracker.ViewModels;
using AppsTracker.Widgets;
using System.ComponentModel.Composition;

namespace AppsTracker
{
    [Export(typeof(IWindow))]
    public partial class MainWindow : Window, IWindow
    {
        private readonly MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;
            this.Loaded += (s, e) =>
            {
                ScaleLoaded();
                this.MaxHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height + 7;
            };
            this.Closing += (s, e) => { mainViewModel.Dispose(); };
        }

        #region Animations
        private void ScaleLoaded()
        {
            DoubleAnimation scaleX = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.5)));
            DoubleAnimation scaleY = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.5)));

            scaleX.SetValue(Storyboard.TargetProperty, this);
            scaleY.SetValue(Storyboard.TargetProperty, this);

            Storyboard story = new Storyboard();
            Storyboard.SetTarget(scaleX, this);
            Storyboard.SetTarget(scaleY, this);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("RenderTransform.ScaleY"));

            this.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            story.Children.Add(scaleX);
            story.Children.Add(scaleY);
            story.Begin(this);
        }

        private void ScaleUnloaded()
        {
            DoubleAnimation scaleX = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.4)));
            DoubleAnimation scaleY = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.4)));

            scaleX.SetValue(Storyboard.TargetProperty, this);
            scaleY.SetValue(Storyboard.TargetProperty, this);

            Storyboard story = new Storyboard();
            Storyboard.SetTarget(scaleX, this);
            Storyboard.SetTarget(scaleY, this);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("RenderTransform.ScaleY"));

            this.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            story.Children.Add(scaleX);
            story.Children.Add(scaleY);
            story.Completed += (s, e) => { this.Close(); };
            story.Begin(this);
        }
        #endregion

        #region Event Handlers


        private void Label_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }



        private void Window_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseButton_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            ScaleUnloaded();
            e.Handled = true;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        #endregion
    }
}
