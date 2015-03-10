using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AppsTracker.Data.Service;

namespace AppsTracker.Views
{
    public partial class Data_logsView : UserControl
    {
        private readonly IXmlSettingsService xmlService;

        public Data_logsView()
        {
            InitializeComponent();
            xmlService = ServiceFactory.Get<IXmlSettingsService>();
            var width = xmlService.LogsViewSettings.VerticalSeparatorPosition;
            if (width != default(double))
                rootLayout.ColumnDefinitions[0].Width = new GridLength(width);
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            // lbData.SelectAll();
        }

        private void Thumb_DragDelta_1(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (rootLayout.ColumnDefinitions[0].ActualWidth + e.HorizontalChange >= 0)
            {
                var width = rootLayout.ColumnDefinitions[0].ActualWidth + e.HorizontalChange;
                rootLayout.ColumnDefinitions[0].Width = new GridLength(width);
                xmlService.LogsViewSettings.VerticalSeparatorPosition = width;
            }
        }
    }
}
