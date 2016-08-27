using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking.Limits;
using AppsTracker.Widgets;

namespace AppsTracker.Views
{
    [Export(typeof(IShell))]
    [ExportMetadata("ShellUse", "Limit toast window")]
    public partial class LimitToastWindow : System.Windows.Window, IShell
    {
        private readonly IXmlSettingsService xmlSettingsService;
        private readonly IAppDurationCalc appDurationCalc;
        private readonly IMediator mediator;

        private readonly MeasureProvider measureProvider;
        private readonly DispatcherTimer timer;

        private AppLimit currentLimit;

        [ImportingConstructor]
        public LimitToastWindow(IXmlSettingsService xmlSettingsService,
                                IAppDurationCalc appDurationCalc,
                                IMediator mediator,
                                MeasureProvider measureProvider)
        {
            this.xmlSettingsService = xmlSettingsService;
            this.appDurationCalc = appDurationCalc;
            this.mediator = mediator;
            this.measureProvider = measureProvider;

            InitializeComponent();

            mediator.Register<AppLimit>(MediatorMessages.APP_LIMIT_REACHED, OnAppLimitReached);

            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            this.Left = bounds.Right - this.Width - 5;
            this.Top = bounds.Bottom + 5;
            timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, OnTimerTick, this.Dispatcher);
            Loaded += (s, e) => HideHelper.RemoveFromAltTab(this);
            Deactivated += (s, e) => Topmost = true;
        }

        private async void OnAppLimitReached(AppLimit limit)
        {
            currentLimit = limit;
            await DisplayAppDuration();
            ShowWindow();
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            if (currentLimit == null)
                return;

            await DisplayAppDuration();
        }


        private async Task DisplayAppDuration()
        {
            long duration = await appDurationCalc.GetDuration(currentLimit.Application.Name, currentLimit.LimitSpan);
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
            var newTop = bounds.Height / measureProvider.ScaleY - this.Height - 35;
            this.Left = bounds.Width / measureProvider.ScaleX - this.Width - 5;

            timer.Start();

            AnimateWindowTopPosition(newTop);
        }

        private void CloseWindow()
        {
            this.Topmost = false;
            this.Opacity = 0.5;

            timer.Stop();

            if (cbDontShow.IsChecked.HasValue && cbDontShow.IsChecked.Value)
            {
                xmlSettingsService.LimitsSettings.DontShowLimits.Add(currentLimit.AppLimitID);
                currentLimit = null;
            }
            
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var newTop = bounds.Bottom + 5;

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

        private class HideHelper
        {
            private const int GwlExstyle = -20;
            private const int WsExToolwindow = 0x80;

            [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
            private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
            private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr value);

            public static void RemoveFromAltTab(IntPtr hwnd)
            {
                var windowStyle = (int)GetWindowLong32(hwnd, GwlExstyle);
                windowStyle |= WsExToolwindow;
                SetWindowLong(hwnd, GwlExstyle, (IntPtr)windowStyle);
            }

            public static void RemoveFromAltTab(System.Windows.Window w)
            {
                RemoveFromAltTab((new WindowInteropHelper(w)).Handle);
            }
        }
    }
}
