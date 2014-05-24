using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Task_Logger_Pro.Controls
{
    public class ScrollingContentControl : ContentControl
    {

        #region Fields

        bool _processing = false;

        DispatcherTimer _centerTimer;

        Storyboard _scrollIn;
        Storyboard _scrollOut;

        Queue<string> _requests = new Queue<string>();

        Label _child;

        #endregion

        #region Dependancy Properties

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
            if(!string.IsNullOrEmpty(e.NewValue as string))
                control.EnqueueNewInfo(e.NewValue as string);
        }

        #endregion

        public ScrollingContentControl()
        {
            InitStoryboards();
            this.RenderTransform = new TranslateTransform();
            this.Visibility = System.Windows.Visibility.Collapsed;
            this.Loaded += (s, e) =>
            {
                _child = this.Content as Label;
                Debug.Assert(_child != null, "Failed to find child label.");
            };
        }

        private void InitStoryboards()
        {
            _scrollIn = FindResource("scrollIn") as Storyboard;
            _scrollOut = FindResource("scrollOut") as Storyboard;
            //Debug.Assert(_scrollIn != null & _scrollOut != null, "Failed to init storyboards");
        }

        private void EnqueueNewInfo(string info)
        {
            _requests.Enqueue(info);
            if (!_processing)
                HandleQueue();
        }

        private void HandleQueue()
        {
            if (_child == null)
                _child = this.Content as Label;
            if (_child == null)
                return;
            _processing = true;

            if (_centerTimer == null)
                InitTimer();
                        
            string info = _requests.Peek();
            _child.Content = info;

            var scrollInClone = _scrollIn.Clone();
            scrollInClone.Completed += (s, e) =>
            {
                _centerTimer.Tick += (sn, ea) =>
                {
                    _centerTimer.Stop();
                    var scrollOutClone = _scrollOut.Clone();
                    scrollOutClone.Completed += (snd, ear) =>
                    {
                        this.Visibility = System.Windows.Visibility.Collapsed;
                        if(_requests.Count >0)
                            _requests.Dequeue();
                        if (_requests.Count == 0)
                        {
                            _processing = false;
                            _child.Content = string.Empty;
                            InfoContent = string.Empty;
                        }
                        else
                        {
                            HandleQueue();
                        }
                    };
                    scrollOutClone.Begin(this);
                };
                _centerTimer.Start();
            };
            scrollInClone.Begin(this);
            this.Visibility = System.Windows.Visibility.Visible;
        }

        private void InitTimer()
        {
            _centerTimer = new DispatcherTimer();
            _centerTimer.Interval = TimeSpan.FromSeconds(CenterTime);
        }
    }
}
