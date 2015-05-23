using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;
using AppsTracker.Widgets;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Limit toast window")]
    public partial class LimitToastWindow : System.Windows.Window, IShell
    {
        private readonly IXmlSettingsService xmlSettingsService;
        private readonly ITrackingService trackingService;
        private readonly DispatcherTimer timer;
        
        [ImportingConstructor]
        public LimitToastWindow(IXmlSettingsService xmlSettingsService,
                                ITrackingService trackingService)
        {
            this.xmlSettingsService = xmlSettingsService;
            this.trackingService = trackingService;

            InitializeComponent();
            
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            this.Left = bounds.Right - this.Width - 5;
            this.Top = bounds.Bottom + 5;
            this.DataContext = (AppLimit)ViewArgument;
            lblTitle.Content = ((AppLimit)ViewArgument).LimitSpan == LimitSpan.Day ?
                "Daily limit reached warning" : "Weekly limit reached warning";
            this.Loaded += (s, e) => ShowWindow();
            timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, OnTimerTick, this.Dispatcher);
            timer.Start();
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            var appLimit = ViewArgument as AppLimit;
            if (appLimit == null)
                return;

            long duration;
            if (appLimit.LimitSpan == LimitSpan.Day)
                duration = await Task.Run(() => trackingService.GetDayDuration(appLimit.Application));
            else
                duration = await Task.Run(() => trackingService.GetWeekDuration(appLimit.Application));

            lblTotalDuration.Content = new TimeSpan(duration).ToString(@"dd\.hh\:mm\:ss");
        }

        private void btnHide_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseWindow();
            timer.Stop();
        }

        private void ShowWindow()
        {
            this.Opacity = 1;
            this.Topmost = true;
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var newTop = bounds.Bottom - this.Height - 35;
            this.Left = bounds.Right - this.Width - 5;

            AnimateWindowTopPosition(newTop, false);
        }

        private void CloseWindow()
        {
            this.Topmost = false;
            this.Opacity = 0.5;
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            this.Left = bounds.Right - this.Width - 5;
            var newTop = bounds.Bottom + 5;

            if(cbDontShow.IsChecked.HasValue && cbDontShow.IsChecked.Value)
                xmlSettingsService.LimitsSettings.DontShowLimits.Add((AppLimit)ViewArgument);

            AnimateWindowTopPosition(newTop, true);
        }

        private void AnimateWindowTopPosition(double newTop, bool closeWindow)
        {
            var fromTop = Double.IsNaN(this.Top) ? System.Windows.Forms.Screen.PrimaryScreen.Bounds.Bottom + 5 : this.Top;
            var animation = new DoubleAnimation(fromTop, newTop, new Duration(TimeSpan.FromSeconds(0.5)));
            animation.SetValue(Storyboard.TargetProperty, this);
            var storyBoard = new Storyboard();
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Window.Top)"));
            storyBoard.Children.Add(animation);
            if (closeWindow)
                storyBoard.Completed += (s, e) => this.Close();

            storyBoard.Begin(this);
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        public object ViewArgument
        {
            get;
            set;
        }
    }
}
