using System.Windows;
using System.Windows.Controls;
using AppsTracker.Data.Service;

namespace AppsTracker.Pages.Views
{
    public partial class KeylogsView : UserControl
    {
        private readonly IXmlSettingsService xmlService;
        public KeylogsView()
        {
            InitializeComponent();
            xmlService = ServiceFactory.Get<IXmlSettingsService>();
            if (xmlService.KeylogsViewSettings.SeparatorPosition != default(double))
                rootLayout.RowDefinitions[0].Height =
                    new GridLength(xmlService.KeylogsViewSettings.SeparatorPosition);
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if ((rootLayout.RowDefinitions[0].ActualHeight + e.VerticalChange) >= 0)
            {
                var height = rootLayout.RowDefinitions[0].ActualHeight + e.VerticalChange;
                rootLayout.RowDefinitions[0].Height = new GridLength(height);
                xmlService.KeylogsViewSettings.SeparatorPosition = height;
            }
        }
    }
}
