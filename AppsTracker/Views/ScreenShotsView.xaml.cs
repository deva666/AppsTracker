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
using AppsTracker.Data.Service;
using AppsTracker.Pages.ViewModels;

namespace AppsTracker.Pages.Views
{

    public partial class ScreenShotsView : UserControl
    {
        private readonly IXmlSettingsService xmlService;
        public ScreenShotsView()
        {
            InitializeComponent();
            xmlService = ServiceFactory.Get<IXmlSettingsService>();
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
