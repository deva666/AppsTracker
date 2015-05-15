using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AppsTracker.Widgets
{
    public partial class ScreenshotViewer : UserControl
    {
        private Storyboard slideOut;
        private Storyboard slideIn;
        private DispatcherTimer timer;

        private readonly static ICommand _previousCommand = new RoutedCommand();
        private readonly static ICommand _pauseCommand = new RoutedCommand();
        private readonly static ICommand _playCommand = new RoutedCommand();
        private readonly static ICommand _forwardCommand = new RoutedCommand();
        private readonly static ICommand _showHideControlsCommand = new RoutedCommand();
        private readonly static ICommand _changeImageViewInfoVisibilityCommand = new RoutedCommand();
        private readonly static ICommand _changeTimerIntervalCommand = new RoutedCommand();

        public static ICommand PreviousCommand { get { return _previousCommand; } }
        public static ICommand PauseCommand { get { return _pauseCommand; } }
        public static ICommand PlayCommand { get { return _playCommand; } }
        public static ICommand ForwardCommand { get { return _forwardCommand; } }
        public static ICommand ShowHideControlsCommand { get { return _showHideControlsCommand; } }
        public static ICommand ChangeImageViewInfoVisibilityCommand { get { return _changeImageViewInfoVisibilityCommand; } }
        public static ICommand ChangeTimerIntervalCommand { get { return _changeTimerIntervalCommand; } }

        public bool Playing
        {
            get { return (bool)GetValue(PlayingProperty); }
            set { SetValue(PlayingProperty, value); }
        }

        public static readonly DependencyProperty PlayingProperty =
            DependencyProperty.Register("Playing", typeof(bool), typeof(ScreenshotViewer), new PropertyMetadata(false));


        public TimeSpan TimerInterval
        {
            get { return (TimeSpan)GetValue(TimerIntervalProperty); }
            set { SetValue(TimerIntervalProperty, value); }
        }

        public static readonly DependencyProperty TimerIntervalProperty =
            DependencyProperty.Register("TimerInterval", typeof(TimeSpan), typeof(ScreenshotViewer), new PropertyMetadata(TimeSpan.FromSeconds(5), new PropertyChangedCallback(UpdateTimer)));


        public int TimerNext
        {
            get { return (int)GetValue(TimerNextProperty); }
            set { SetValue(TimerNextProperty, value); }
        }

        public static readonly DependencyProperty TimerNextProperty =
            DependencyProperty.Register("TimerNext", typeof(int), typeof(ScreenshotViewer), new PropertyMetadata(0));



        public ScreenshotViewer()
        {
            InitializeComponent();
            InitializeCommandBindings();
            timer = new DispatcherTimer();
            lbImages.SelectionChanged += lbImages_SelectionChanged;
            timer.Interval = TimerInterval;
            timer.Tick += timer_Tick;
            timer.Start();
            Playing = true;
            this.Loaded += (s, e) =>
            {
                slideOut = FindResource("slideOut") as Storyboard;
                slideIn = FindResource("slideIn") as Storyboard;
                Debug.Assert(slideIn != null && slideOut != null, "Failed to find storyboards!");
                slideOut.Completed += SlideOutCompleted;
            };
        }

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(PreviousCommand, PreviousCommandExecuted));
            CommandBindings.Add(new CommandBinding(PauseCommand, PauseCommandExecuted));
            CommandBindings.Add(new CommandBinding(PlayCommand, PlayCommandExecuted));
            CommandBindings.Add(new CommandBinding(ForwardCommand, ForwardCommandExecuted));
            CommandBindings.Add(new CommandBinding(ShowHideControlsCommand, ShowHideControlsCommandExecuted));
            CommandBindings.Add(new CommandBinding(ChangeImageViewInfoVisibilityCommand, ChangeImageViewInfoVisibility));
            CommandBindings.Add(new CommandBinding(ChangeTimerIntervalCommand, ChangeInterval));
        }

        private void ChangeInterval(object sender, ExecutedRoutedEventArgs e)
        {
            switch (e.Parameter as string)
            {
                case "3":
                    this.TimerInterval = new TimeSpan(0, 0, 3);
                    break;
                case "5":
                    this.TimerInterval = new TimeSpan(0, 0, 5);
                    break;
                case "10":
                    this.TimerInterval = new TimeSpan(0, 0, 10);
                    break;
                case "20":
                    this.TimerInterval = new TimeSpan(0, 0, 20);
                    break;
                case "30":
                    this.TimerInterval = new TimeSpan(0, 0, 30);
                    break;
                default:
                    break;
            }
            rbInterval.IsChecked = false;
        }

        private void ChangeImageViewInfoVisibility(object sender, ExecutedRoutedEventArgs e)
        {
            if (imageView.IsInfoVisible)
                imageView.IsInfoVisible = false;
            else
                imageView.IsInfoVisible = true;
        }

        private void ShowHideControlsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (controlsPanel.FaderVisibility) controlsPanel.FaderVisibility = false;
            else controlsPanel.FaderVisibility = true;
        }

        private void ForwardCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (lbImages.SelectedIndex < lbImages.Items.Count - 1)
            {
                if (Playing)
                    timer.Reset();
                lbImages.SelectedIndex++;
                lbImages.ScrollIntoView(lbImages.SelectedItem);
            }

        }

        private void PlayCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            timer.Start();
            Playing = true;
        }

        private void PauseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            timer.Stop();
            Playing = false;
        }

        private void PreviousCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (lbImages.SelectedIndex > 1)
            {
                if (Playing)
                    timer.Reset();
                lbImages.SelectedIndex--;
                lbImages.ScrollIntoView(lbImages.SelectedItem);
            }
        }

        private static void UpdateTimer(DependencyObject dpobject, DependencyPropertyChangedEventArgs args)
        {
            (dpobject as ScreenshotViewer).timer.Interval = (TimeSpan)args.NewValue;
        }

        void lbImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Playing)
            {
                timer.Reset();
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (lbImages.SelectedIndex < lbImages.Items.Count - 1)
            {
                slideOut.Begin();
            }
        }

        private void SlideOutCompleted(object sender, EventArgs args)
        {
            lbImages.SelectedIndex++;
            lbImages.ScrollIntoView(lbImages.SelectedItem);
            slideIn.Begin();
        }

    }
}
