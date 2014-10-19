using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Task_Logger_Pro.Controls
{
    public class FaderLabel : ContentControl
    {
        public FaderLabel()
        {
            this.Loaded += FaderLabel_Loaded;
            this.Unloaded += FaderLabel_Unloaded;
            this.IsVisibleChanged += FaderLabel_IsVisibleChanged;
        }

        void FaderLabel_IsVisibleChanged( object sender, System.Windows.DependencyPropertyChangedEventArgs e )
        {

            if ( this.IsVisible ) VisualStateManager.GoToState( this, "AfterLoaded", true ) ;
            else if ( !this.IsVisible ) VisualStateManager.GoToState( this, "AfterUnLoaded", true );
        }

        void FaderLabel_Unloaded( object sender, System.Windows.RoutedEventArgs e )
        {
            VisualStateManager.GoToState( this, "AfterUnLoaded", true );
        }

        void FaderLabel_Loaded( object sender, System.Windows.RoutedEventArgs e )
        {
            VisualStateManager.GoToState( this, "AfterLoaded", true );
        }
    }
}
