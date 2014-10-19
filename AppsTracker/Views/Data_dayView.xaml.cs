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
    /// <summary>
    /// Interaction logic for Data_dayView.xaml
    /// </summary>
    public partial class Data_dayView : UserControl
    {
        public Data_dayView()
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
