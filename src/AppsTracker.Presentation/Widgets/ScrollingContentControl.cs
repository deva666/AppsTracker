using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AppsTracker.Widgets
{
    public class ScrollingContentControl : ContentControl
    {
        private bool isQueing = false;

        private DispatcherTimer centerTimer;

        private readonly Storyboard scrollIn;
        private readonly Storyboard scrollOut;

        private readonly Queue<string> requests = new Queue<string>();

        private Label child;

        public double CenterTime
        {
            get { return (double)GetValue(CenterTimeProperty); }
            set { SetValue(CenterTimeProperty, value); }
        }

        public static readonly DependencyProperty CenterTimeProperty =
            DependencyProperty.Register("CenterTime", typeof(double), typeof(ScrollingContentControl), new PropertyMetadata(2.0d));

        public string InfoContent
        {
            get { return (string)GetValue(InfoContentProperty); }
            set { SetValue(InfoContentProperty, value); }
        }

        public static readonly DependencyProperty InfoContentProperty =
            DependencyProperty.Register("InfoContent", typeof(string), typeof(ScrollingContentControl), new PropertyMetadata(string.Empty, InfoContentCallback));

        private static void InfoContentCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScrollingContentControl control = d as ScrollingContentControl;

            if (!string.IsNullOrEmpty(e.NewValue as string))
                control.EnqueueNewInfo(e.NewValue as string);
        }

        public ScrollingContentControl()
        {
            scrollIn = (Storyboard)FindResource("scrollIn");
            scrollOut = (Storyboard)FindResource("scrollOut");
            this.RenderTransform = new TranslateTransform();
            this.Visibility = System.Windows.Visibility.Collapsed;
            this.Loaded += (s, e) =>
            {
                child = this.Content as Label;
                Debug.Assert(child != null, "Failed to find child label.");
            };
        }

        
        private void EnqueueNewInfo(string info)
        {
            if (string.IsNullOrWhiteSpace(info))
                return;
            requests.Enqueue(info);
            if (!isQueing)
                HandleQueue();
        }

        private void HandleQueue()
        {
            if (child == null)
                return;
            isQueing = true;

            InitTimer();

            string info = requests.Dequeue();
            child.Content = info;

            var scrollInClone = scrollIn.Clone();
            scrollInClone.Completed += (s, e) =>
            {
                centerTimer.Tick -= TimerTick;
                centerTimer.Tick += TimerTick;
                centerTimer.Start();
            };
            scrollInClone.Begin(this);
            this.Visibility = System.Windows.Visibility.Visible;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            centerTimer.Tick -= TimerTick;
            centerTimer.Stop();

            var scrollOutClone = scrollOut.Clone();
            scrollOutClone.Completed += (snd, args) =>
            {
                this.Visibility = System.Windows.Visibility.Collapsed;
                child.Content = String.Empty;
                if (requests.Count == 0)
                    isQueing = false;
                else
                    HandleQueue();
            };
            scrollOutClone.Begin(this);
        }

        private void InitTimer()
        {
            if (centerTimer == null)
                centerTimer = new DispatcherTimer();
            centerTimer.Interval = TimeSpan.FromSeconds(CenterTime);
        }
    }
}
