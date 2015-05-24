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
        private readonly IMediator mediator;

        private readonly DispatcherTimer timer;

        private AppLimit currentLimit;

        [ImportingConstructor]
        public LimitToastWindow(IXmlSettingsService xmlSettingsService,
                                ITrackingService trackingService,
                                IMediator mediator)
        {
            this.xmlSettingsService = xmlSettingsService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            InitializeComponent();

            mediator.Register<AppLimit>(MediatorMessages.APP_LIMIT_REACHED, OnAppLimitReached);

            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            this.Left = bounds.Right - this.Width - 5;
            this.Top = bounds.Bottom + 5;
            timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, OnTimerTick, this.Dispatcher);
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            if (currentLimit == null)
                return;
            await DisplayAppDuration();            
        }


        private async void OnAppLimitReached(AppLimit limit)
        {
            currentLimit = limit;
            await DisplayAppDuration();
            ShowWindow();
        }

        private async Task DisplayAppDuration()
        {
            long duration;
            if (currentLimit.LimitSpan == LimitSpan.Day)
                duration = await Task.Run(() => trackingService.GetDayDuration(currentLimit.Application));
            else
                duration = await Task.Run(() => trackingService.GetWeekDuration(currentLimit.Application));
            lblTotalDuration.Content = new TimeSpan(duration).ToString(@"dd\.hh\:mm\:ss");
        }


        private void ShowWindow()
        {
            this.DataContext = currentLimit;
            lblTitle.Content = currentLimit.LimitSpan == LimitSpan.Day ?
                "Daily limit reached warning" : "Weekly limit reached warning";
            this.Opacity = 1;
            this.Topmost = true;
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var newTop = bounds.Bottom - this.Height - 35;
            this.Left = bounds.Right - this.Width - 5;

            AnimateWindowTopPosition(newTop);
        }

        private void CloseWindow()
        {
            this.Topmost = false;
            this.Opacity = 0.5;
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            this.Left = bounds.Right - this.Width - 5;
            var newTop = bounds.Bottom + 5;

            if (cbDontShow.IsChecked.HasValue && cbDontShow.IsChecked.Value)
                xmlSettingsService.LimitsSettings.DontShowLimits.Add(currentLimit);

            AnimateWindowTopPosition(newTop);
        }

        private void AnimateWindowTopPosition(double newTop)
        {
            var fromTop = Double.IsNaN(this.Top) ? System.Windows.Forms.Screen.PrimaryScreen.Bounds.Bottom + 5 : this.Top;
            var animation = new DoubleAnimation(fromTop, newTop, new Duration(TimeSpan.FromSeconds(0.5)));
            animation.SetValue(Storyboard.TargetProperty, this);
            var storyBoard = new Storyboard();
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Window.Top)"));
            storyBoard.Children.Add(animation);
            storyBoard.Begin(this);
        }

        private void btnHide_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseWindow();
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
