#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Helpers
{
    [Export(typeof(IScreenshotFactory))]
    internal sealed class ScreenshotFactory : IScreenshotFactory
    {
        private Bitmap CaptureScreen(out Size size)
        {
            try
            {
                size = Size.Empty;
                NativeMethods.RECT rect = new NativeMethods.RECT();
                IntPtr hForWnd = NativeMethods.GetForegroundWindow();
                if (hForWnd == IntPtr.Zero) 
                    return null;
                NativeMethods.GetWindowRect(hForWnd, ref rect);
                IntPtr hwnd = NativeMethods.GetDC(IntPtr.Zero);
                if (hwnd == IntPtr.Zero)
                    return null;
                
                using (Graphics graphics = Graphics.FromHdc(hwnd))
                {
                    Bitmap tempImg = new Bitmap(rect.Width, rect.Height, graphics);
                    using (Graphics tempGraphics = Graphics.FromImage(tempImg))
                    {
                        tempGraphics.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
                    }
                    size = new Size(rect.Width, rect.Height);
                    return tempImg;
                }
            }
            catch 
            {
                size = Size.Empty;
                return null;
            }

        }

        public Screenshot CreateScreenshot()
        {
            Size size;
            Screenshot screenshot;

            using (Bitmap screenshotImage = CaptureScreen(out size))
            {
                if (screenshotImage == null)
                    return null;

                screenshot = new Screenshot(size, screenshotImage);
            }

            return screenshot;
        }
    }
}
