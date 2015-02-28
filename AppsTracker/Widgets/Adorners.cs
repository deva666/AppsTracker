using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppsTracker.Controls
{
    public class HideAdorner : Adorner
    {
        double width, height;

        public HideAdorner(UIElement adornedElement) : base(adornedElement)
        {
            width = this.AdornedElement.DesiredSize.Width;
            height = this.AdornedElement.DesiredSize.Height;
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            drawingContext.DrawImage(new BitmapImage(new Uri(@"pack://application:,,,/Resources/down.png")), new Rect(0, 0, width, height));
            base.OnRender(drawingContext);
        }
    }
}
