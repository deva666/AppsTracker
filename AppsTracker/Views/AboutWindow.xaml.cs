using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AppsTracker.ViewModels;

namespace AppsTracker.Widgets
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "About window")]
    public partial class AboutWindow : Window, IShell
    {
        [ImportingConstructor]
        public AboutWindow(AboutWindowViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.Loaded += (s, e) => ScaleLoaded();
        }


        #region Animations

        private void ScaleLoaded()
        {
            DoubleAnimation scaleX = new DoubleAnimation(0.3, 1.0, new Duration(TimeSpan.FromSeconds(0.5)));
            DoubleAnimation scaleY = new DoubleAnimation(0.3, 1.0, new Duration(TimeSpan.FromSeconds(0.5)));

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
            DoubleAnimation scaleX = new DoubleAnimation(1.0, 0.1, new Duration(TimeSpan.FromSeconds(0.4)));
            DoubleAnimation scaleY = new DoubleAnimation(1.0, 0.1, new Duration(TimeSpan.FromSeconds(0.4)));

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

        private void lblOK_Click(object sender, RoutedEventArgs e)
        {
            ScaleUnloaded();
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("mailto:info@theappstracker.com");
            }
            catch (Exception ex)
            {
                FileLogger.Instance.Log(ex);
            }
        }

        private void TextBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.theappstracker.com");
            }
            catch (Exception ex)
            {

                FileLogger.Instance.Log(ex);
            }
        }


        public object ViewArgument
        {
            get;
            set;
        }
    }
}
