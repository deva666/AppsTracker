using System;
using System.ComponentModel.Composition;
using System.Drawing;
using AppsTracker.Common.Utils;

namespace AppsTracker.Widgets
{
    [Export]
    public sealed class MeasureProvider
    {
        private const double DPI = 96;
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;

        public double ScaleX { get { return GetScaleX(); } }

        public double ScaleY { get { return GetScaleY(); } }

        private double GetScaleX()
        {
            var graphics = Graphics.FromHwnd(IntPtr.Zero);
            var deviceContext = graphics.GetHdc();
            return NativeMethods.GetDeviceCaps(deviceContext, LOGPIXELSX) / DPI;
        }

        private double GetScaleY()
        {
            var graphics = Graphics.FromHwnd(IntPtr.Zero);
            var deviceContext = graphics.GetHdc();
            return NativeMethods.GetDeviceCaps(deviceContext, LOGPIXELSY) / DPI;
        }
    }
}
