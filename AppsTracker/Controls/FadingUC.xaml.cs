using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppsTracker.Controls
{
    /// <summary>
    /// Interaction logic for FadingUC.xaml
    /// </summary>
    public partial class FadingUC : UserControl
    {


        public int LabelFontSize
        {
            get { return ( int ) GetValue( LabelFontSizeProperty ); }
            set { SetValue( LabelFontSizeProperty, value ); }
        }

        // Using a DependencyProperty as the backing store for LabelFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register( "LabelFontSize", typeof( int ), typeof( FadingUC ), new PropertyMetadata( 12 ) );

        

        public string StringContent
        {
            get { return ( string ) GetValue( StringContentProperty ); }
            set { SetValue( StringContentProperty, value ); }
        }

        // Using a DependencyProperty as the backing store for StringContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringContentProperty =
            DependencyProperty.Register( "StringContent", typeof( string ), typeof( FadingUC ), new PropertyMetadata( "" ) );

        
        public FadingUC()
        {
            InitializeComponent();
            this.Loaded += FadingUC_Loaded;
            this.Unloaded += FadingUC_Unloaded;
            this.IsVisibleChanged += FadingUC_IsVisibleChanged;
        }

        void FadingUC_IsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            if ( this.IsVisible )  VisualStateManager.GoToState( this, "AfterLoaded", true ) ;
            else if ( !this.IsVisible ) VisualStateManager.GoToState( this, "AfterUnLoaded", true );
        }

        void FadingUC_Unloaded( object sender, RoutedEventArgs e )
        {
            VisualStateManager.GoToState( this, "AfterUnLoaded", true );
        }

        void FadingUC_Loaded( object sender, RoutedEventArgs e )
        {
            VisualStateManager.GoToState( this, "AfterLoaded", true );
        }
    }
}
