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

namespace AppsTracker.Views
{
   
    public partial class ImageView : UserControl
    {
        public static ICommand ChangeInfoVisibilityCommand = new RoutedCommand();
        public UIElement SynchronizationElement
        {
            get { return (UIElement)GetValue(SynchronizationElementProperty); }
            set { SetValue(SynchronizationElementProperty, value); }
        }

        public static readonly DependencyProperty SynchronizationElementProperty =
            DependencyProperty.Register("SynchronizationElement", typeof(UIElement), typeof(ImageView), new PropertyMetadata(null));

        public bool IsInfoVisible
        {
            get { return (bool)GetValue(IsInfoVisibleProperty); }
            set { SetValue(IsInfoVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsInfoVisibleProperty =
            DependencyProperty.Register("IsInfoVisible", typeof(bool), typeof(ImageView), new PropertyMetadata(true));

        
        public ImageView()
        {
           InitializeComponent();
           CommandBindings.Add(new CommandBinding(ChangeInfoVisibilityCommand, ChangeInfoVisibility));
        }

        private void ChangeInfoVisibility(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.IsInfoVisible)
                IsInfoVisible = false;
            else
                IsInfoVisible = true;
        }

    }
}
