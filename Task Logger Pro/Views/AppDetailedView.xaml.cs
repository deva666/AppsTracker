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

namespace Task_Logger_Pro.Views
{
    /// <summary>
    /// Interaction logic for AppDetailedView.xaml
    /// </summary>
    public partial class AppDetailedView : UserControl
    {
        public AppDetailedView()
        {
            InitializeComponent();
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if ((rootLayout.RowDefinitions[1].ActualHeight + e.VerticalChange) >= 0)
            {
                rootLayout.RowDefinitions[1].Height = new GridLength(rootLayout.RowDefinitions[1].ActualHeight + e.VerticalChange);
            }
        }
    }
}
