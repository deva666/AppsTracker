using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.Views
{
    public partial class AppDetailsView : UserControl
    {
        private readonly IXmlSettingsService xmlService;

        public AppDetailsView()
        {
            InitializeComponent();
            xmlService = ServiceResolver.Instance.Resolve<IXmlSettingsService>();
            var width = xmlService.LogsViewSettings.VerticalSeparatorPosition;
            if (width != default(double))
                rootLayout.ColumnDefinitions[0].Width = new GridLength(width);
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
