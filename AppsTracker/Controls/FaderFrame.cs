using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Task_Logger_Pro.Controls
{
    public class FaderFrame : Frame
    {
        private bool _allowDirectNavigation = false;
        private ContentPresenter _contentPresenter = null;
        private NavigatingCancelEventArgs _navArgs = null;


        public bool IsActive
        {
            get { return ( bool ) GetValue( IsActiveProperty ); }
            set { SetValue( IsActiveProperty, value ); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register( "IsActive", typeof( bool ), typeof( FaderFrame ), new PropertyMetadata( false ) );


        public Duration FadeDuration
        {
            get { return ( Duration ) GetValue( FadeDurationProperty ); }
            set { SetValue( FadeDurationProperty, value ); }
        }

        public static readonly DependencyProperty FadeDurationProperty =
            DependencyProperty.Register( "FadeDuration", typeof( Duration ), typeof( FaderFrame ),
            new FrameworkPropertyMetadata( new Duration( TimeSpan.FromMilliseconds( 500 ) ) ) );

        public FaderFrame()
            : base()
        {
            Navigating += new NavigatingCancelEventHandler( FaderFrame_Navigating );
            Navigated += FaderFrame_Navigated;
            this.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
        }

        void FaderFrame_Navigated( object sender, NavigationEventArgs e )
        {
            this.NavigationService.RemoveBackEntry();
        }

        public override void OnApplyTemplate()
        {
            _contentPresenter = GetTemplateChild( "PART_FrameCP" ) as ContentPresenter;
            base.OnApplyTemplate();
        }

        void FaderFrame_Navigating( object sender, NavigatingCancelEventArgs e )
        {
            if ( Content != null && !_allowDirectNavigation && _contentPresenter != null )
            {
                e.Cancel = true;
                _navArgs = e;
                _contentPresenter.IsHitTestVisible = false;
                DoubleAnimation da = new DoubleAnimation( 0.0d, FadeDuration );
                da.DecelerationRatio = 1.0d;
                da.Completed += FadeOutCompleted;
                _contentPresenter.BeginAnimation( OpacityProperty, da );
                this.IsActive = false;
            }
            _allowDirectNavigation = false;
        }

        private void FadeOutCompleted( object sender, EventArgs e )
        {
            ( sender as AnimationClock ).Completed -= FadeOutCompleted;
            if ( _contentPresenter != null )
            {
                _contentPresenter.IsHitTestVisible = true;

                _allowDirectNavigation = true;
                switch ( _navArgs.NavigationMode )
                {
                    case NavigationMode.New:

                        if ( _navArgs.Uri == null )
                        {
                            NavigationService.Navigate( _navArgs.Content );
                        }
                        else
                        {
                            NavigationService.Navigate( _navArgs.Uri );
                        }
                        break;

                    case NavigationMode.Back:
                        NavigationService.GoBack();
                        break;

                    case NavigationMode.Forward:
                        NavigationService.GoForward();
                        break;

                    case NavigationMode.Refresh:
                        NavigationService.Refresh();
                        break;
                }

                Dispatcher.BeginInvoke( DispatcherPriority.Loaded, ( ThreadStart ) delegate()
                    {
                        DoubleAnimation da = new DoubleAnimation( 1.0d, FadeDuration );
                        da.AccelerationRatio = 1.0d;
                        _contentPresenter.BeginAnimation( OpacityProperty, da );
                    } );
                this.IsActive = true;
            }
        }
    }
}
