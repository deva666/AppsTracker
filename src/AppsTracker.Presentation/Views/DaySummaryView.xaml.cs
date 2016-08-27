using System.Windows;
using System.Windows.Controls;
using AppsTracker.Data.Repository;
using AppsTracker.ServiceLocation;

namespace AppsTracker.Views
{
    public partial class DaySummaryView : UserControl
    {
        private readonly IUserSettingsService xmlService;

        public DaySummaryView()
        {
            InitializeComponent();
            xmlService = ServiceLocator.Instance.Resolve<IUserSettingsService>();
            var height = xmlService.DaysViewSettings.SeparatorPosition;
            if (height != default(double))
                rootLayout.RowDefinitions[1].Height = new GridLength(height);
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if ((rootLayout.RowDefinitions[1].ActualHeight + e.VerticalChange) >= 0)
            {
                var height = rootLayout.RowDefinitions[1].ActualHeight + e.VerticalChange;
                rootLayout.RowDefinitions[1].Height = new GridLength(height);
                xmlService.DaysViewSettings.SeparatorPosition = height;
            }
        }
    }
}
