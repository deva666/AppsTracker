using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using AppsTracker.Logging;
using AppsTracker.Pages.ViewModels;

namespace AppsTracker.Pages.Views
{
    public partial class Data_logsView : UserControl
    {
        public Data_logsView()
        {
            InitializeComponent();
        }
              
        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            // lbData.SelectAll();
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (rootLayout.ColumnDefinitions[1].ActualWidth + e.HorizontalChange >= 0)
            {
                rootLayout.ColumnDefinitions[1].Width = new GridLength(rootLayout.ColumnDefinitions[1].ActualWidth + e.HorizontalChange);
            }
        }

        private void Thumb_DragDelta_1(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (rootLayout.ColumnDefinitions[0].ActualWidth + e.HorizontalChange >= 0)
            {
                rootLayout.ColumnDefinitions[0].Width = new GridLength(rootLayout.ColumnDefinitions[0].ActualWidth + e.HorizontalChange);
            }
        }
    }
}
