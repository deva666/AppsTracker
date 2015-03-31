using System.Windows;
using System.Windows.Controls;
using AppsTracker.Data.Service;
using AppsTracker.ServiceLocation;

namespace AppsTracker.Views
{

    public partial class ScreenshotsView : UserControl
    {
        private readonly IXmlSettingsService xmlService;
        public ScreenshotsView()
        {
            InitializeComponent();
            xmlService = ServiceLocator.Instance.Resolve<IXmlSettingsService>();
            var height = xmlService.ScreenshotsViewSettings.SeparatorPosition;
            if (height != default(double))
                rootLayout.RowDefinitions[0].Height = new GridLength(height);
        }

        private void Thumb_DragDelta_1(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if ((rootLayout.RowDefinitions[0].ActualHeight + e.VerticalChange) >= 0)
            {
                var height = rootLayout.RowDefinitions[0].ActualHeight + e.VerticalChange;
                rootLayout.RowDefinitions[0].Height = new GridLength(height);
                xmlService.ScreenshotsViewSettings.SeparatorPosition = height;
            }
        }
    }
}
