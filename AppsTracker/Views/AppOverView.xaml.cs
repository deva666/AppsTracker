using System.Windows;
using System.Windows.Controls;
using AppsTracker.Data.Service;
using AppsTracker.ServiceLocation;

namespace AppsTracker.Widgets
{
    public partial class AppOverView : UserControl
    {
        private readonly IXmlSettingsService xmlService;

        public AppOverView()
        {
            InitializeComponent();
            xmlService = ServiceLocator.Instance.Resolve<IXmlSettingsService>();
            var height = xmlService.LogsViewSettings.HorizontalSeparatorPosition;
            if (height != default(double))
                rootLayout.RowDefinitions[1].Height = new GridLength(height);
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if ((rootLayout.RowDefinitions[1].ActualHeight + e.VerticalChange) >= 0)
            {
                var height = rootLayout.RowDefinitions[1].ActualHeight + e.VerticalChange;
                rootLayout.RowDefinitions[1].Height = new GridLength(height);
                xmlService.LogsViewSettings.HorizontalSeparatorPosition = height;
            }
        }
    }
}
