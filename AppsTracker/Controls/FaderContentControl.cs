using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppsTracker.Controls
{
    public class FaderContentControl : ContentControl
    {
        Storyboard fadeOut;
        Storyboard fadeIn;
        Shape paintArea;
        ContentPresenter mainArea;

        public bool FadeVertically
        {
            get { return (bool)GetValue(FadeVerticallyProperty); }
            set { SetValue(FadeVerticallyProperty, value); }
        }

        public static readonly DependencyProperty FadeVerticallyProperty =
            DependencyProperty.Register("FadeVertically", typeof(bool), typeof(FaderContentControl), new PropertyMetadata(false, new PropertyChangedCallback(FadeVerticalCallBack)));


        public bool FaderVisibility
        {
            get { return (bool)GetValue(FaderVisibilityProperty); }
            set { SetValue(FaderVisibilityProperty, value); }
        }

        public static readonly DependencyProperty FaderVisibilityProperty =
            DependencyProperty.Register("FaderVisibility", typeof(bool), typeof(FaderContentControl), new PropertyMetadata(true, new PropertyChangedCallback(FaderVisibilityCallBack)));


        private static void FaderVisibilityCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FaderContentControl control = d as FaderContentControl;
            control.ChangeVisibility((bool)e.NewValue);
        }

        private static void FadeVerticalCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FaderContentControl control = d as FaderContentControl;
            control.InitStoryBoards((bool)e.NewValue);
        }

        public FaderContentControl()
        {
            this.Loaded += FaderContentControl_Loaded;
            this.Unloaded += FaderContentControl_Unloaded;
            this.IsVisibleChanged += FaderContentControl_IsVisibleChanged;
            InitStoryBoards(FadeVertically);
        }

        private void InitStoryBoards(bool fadeVertical)
        {
            fadeIn = fadeVertical ? FindResource("fadeInVertical") as Storyboard : FindResource("fadeIn") as Storyboard;
            fadeOut = fadeVertical ? FindResource("fadeOutVertical") as Storyboard : FindResource("fadeOut") as Storyboard;
        }

        private Brush CreateBrushFromVisual(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");
            var target = new RenderTargetBitmap((int)this.ActualWidth, (int)this.ActualHeight,
                                                96, 96, PixelFormats.Pbgra32);
            target.Render(visual);
            var brush = new ImageBrush(target);
            brush.Freeze();
            return brush;
        }

        private void ChangeVisibility(bool visible)
        {
            if (!visible)
            {
                Storyboard fadeOutClone = fadeOut.Clone();
                fadeOutClone.Completed += (s, e) => { this.Visibility = System.Windows.Visibility.Collapsed; };
                fadeOutClone.Begin(this);
            }
            else
            {
                Storyboard fadeInClone = fadeIn.Clone();
                this.Visibility = System.Windows.Visibility.Visible;
                fadeInClone.Begin(this);
            }

        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (oldContent != null && newContent != null)
            {
                paintArea.Fill = CreateBrushFromVisual(mainArea);
                paintArea.Visibility = System.Windows.Visibility.Visible;
                mainArea.Visibility = System.Windows.Visibility.Collapsed;
                Storyboard fadeOutClone = fadeOut.Clone();
                Storyboard fadeInClone = fadeIn.Clone();
                fadeOutClone.Completed += (s, e) =>
                {
                    fadeInClone.Begin(mainArea);
                    paintArea.Visibility = System.Windows.Visibility.Collapsed; 
                    mainArea.Visibility = System.Windows.Visibility.Visible;
                };
                fadeOutClone.Begin(paintArea);
            }

            base.OnContentChanged(oldContent, newContent);

        }

        public override void OnApplyTemplate()
        {
            if (FadeVertically)
                VisualStateManager.GoToState(this, "AfterLoadedVertical", true);
            else
                VisualStateManager.GoToState(this, "AfterLoaded", true);

            base.OnApplyTemplate();

            paintArea = Template.FindName("paintArea", this) as Shape;
            mainArea = Template.FindName("mainArea", this) as ContentPresenter;

            paintArea.RenderTransform = new TranslateTransform();
            mainArea.RenderTransform = new TranslateTransform();
        }

        void FaderContentControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (FadeVertically)
            {
                if (this.IsVisible)
                    VisualStateManager.GoToState(this, "AfterLoadedVertical", true);
                else
                    VisualStateManager.GoToState(this, "AfterUnLoaded", true);
            }
            else
            {
                if (this.IsVisible)
                    VisualStateManager.GoToState(this, "AfterLoaded", true);
                else
                    VisualStateManager.GoToState(this, "AfterUnLoaded", true);
            }
        }

        void FaderContentControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (FadeVertically)
                VisualStateManager.GoToState(this, "AfterUnLoadedVertical", true);
            else
                VisualStateManager.GoToState(this, "AfterUnLoaded", true);
        }

        void FaderContentControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (FadeVertically)
                VisualStateManager.GoToState(this, "AfterLoadedVertical", true);
            else
                VisualStateManager.GoToState(this, "AfterLoaded", true);
        }
    }




}
