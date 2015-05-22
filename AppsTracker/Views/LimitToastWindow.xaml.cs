using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media.Animation;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Widgets;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Limit toast window")]
    public partial class LimitToastWindow : System.Windows.Window, IShell
    {
        private readonly IMediator mediator;

        [ImportingConstructor]
        public LimitToastWindow(IMediator mediator)
        {
            InitializeComponent();
            this.mediator = mediator;
            this.mediator.Register<Tuple<AppLimit, long>>(MediatorMessages.APP_LIMIT_REACHED, OnAppLimitReached);            
            HideWindow();
        }

        private void OnAppLimitReached(Tuple<AppLimit, long> tuple)
        {
            if (Dispatcher.Thread.ManagedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                Dispatcher.BeginInvoke(new Action(() => OnAppLimitReached(tuple)));
                return;
            }
            this.DataContext = tuple.Item1;
            lblTotalDuration.Content = new TimeSpan(tuple.Item2).ToString(@"dd\.hh\:mm\:ss");
            ShowWindow();
        }

        private void btnHide_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HideWindow();
        }

        private void ShowWindow()
        {
            if (Dispatcher.Thread.ManagedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                Dispatcher.BeginInvoke(new Action(ShowWindow));
                return;
            }
            this.Opacity = 1;
            this.Topmost = true;
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var newTop = bounds.Bottom - this.Height - 35;
            this.Left = bounds.Right - this.Width - 5;

            AnimateWindowTopPosition(newTop);
        }

        private void HideWindow()
        {
            if (Dispatcher.Thread.ManagedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                Dispatcher.BeginInvoke(new Action(ShowWindow));
                return;
            }
            this.Topmost = false;
            this.Opacity = 0.5;
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            this.Left = bounds.Right - this.Width - 5;
            var newTop = bounds.Bottom + 5;

            AnimateWindowTopPosition(newTop);
        }

        private void AnimateWindowTopPosition(double newTop)
        {
            var fromTop = Double.IsNaN(this.Top) ? 3000 : this.Top;
            var animation = new DoubleAnimation(fromTop, newTop, new Duration(TimeSpan.FromSeconds(0.5)));
            animation.SetValue(Storyboard.TargetProperty, this);
            var storyBoard = new Storyboard();
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Window.Top)"));
            storyBoard.Children.Add(animation);
            storyBoard.Begin(this);
        }


        public object ViewArgument
        {
            get;
            set;
        }
    }
}
