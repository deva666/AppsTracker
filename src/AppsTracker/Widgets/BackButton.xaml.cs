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

namespace AppsTracker.Widgets
{

    public partial class BackButton : UserControl
    {
                
        public Brush EllipseBackgroundBrush
        {
            get { return (Brush)GetValue(EllipseBackgroundBrushProperty); }
            set { SetValue(EllipseBackgroundBrushProperty, value); }
        }

        public static readonly DependencyProperty EllipseBackgroundBrushProperty =
            DependencyProperty.Register("EllipseBackgroundBrush", typeof(Brush), typeof(BackButton), new PropertyMetadata(Brushes.Black));


        public BackButton()
        {
            InitializeComponent();
            this.Unloaded += BackButton_Unloaded;
            this.IsVisibleChanged += BackButton_IsVisibleChanged;
        }

        void BackButton_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible) VisualStateManager.GoToState(this, "AfterLoaded", true);
            else if (!this.IsVisible) VisualStateManager.GoToState(this, "AfterUnLoaded", true);
        }

        void BackButton_Unloaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "AfterUnLoaded", true);
        }

    }
}
