using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Task_Logger_Pro.Pages.ViewModels;

namespace Task_Logger_Pro.Pages.Views
{

    public partial class ScreenShotsView : UserControl
    {
        public ScreenShotsView()
        {
            InitializeComponent();
        }

        private void Thumb_DragDelta_1(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if ((rootLayout.RowDefinitions[0].ActualHeight + e.VerticalChange) >= 0)
            {
                rootLayout.RowDefinitions[0].Height = new GridLength(rootLayout.RowDefinitions[0].ActualHeight + e.VerticalChange);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mainContentHolder.ScrollIntoView(mainContentHolder.SelectedItem);
        }
    }
}
