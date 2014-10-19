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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppsTracker.Controls
{
    /// <summary>
    /// Interaction logic for EULAWindow.xaml
    /// </summary>
    public partial class EULAWindow : Window
    {
        public EULAWindow()
        {
            InitializeComponent();
        }

        private void FadeUnloaded()
        {
            DoubleAnimation fadeOut = new DoubleAnimation( 1.0, 0.0, new Duration( TimeSpan.FromSeconds( 0.6 ) ) );

            fadeOut.SetValue( Storyboard.TargetProperty, this );

            Storyboard story = new Storyboard();
            Storyboard.SetTarget( fadeOut, this );
            Storyboard.SetTargetProperty( fadeOut, new PropertyPath( "Opacity" ) );

            story.Children.Add( fadeOut );
            story.Completed += ( s, e ) => { this.Close(); };
            story.Begin( this );
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            this.DialogResult = true;
            FadeUnloaded();
        }

        private void Button_Click_1( object sender, RoutedEventArgs e )
        {
            this.DialogResult = false;
            FadeUnloaded();
        }
    }
}
