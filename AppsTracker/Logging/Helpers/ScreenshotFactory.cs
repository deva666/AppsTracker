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
using AppsTracker.Data.Models;

namespace AppsTracker.Logging.Helpers
{
    [Export(typeof(IScreenshotFactory))]
    internal sealed class ScreenshotFactory : IScreenshotFactory
    {
        private Bitmap TakeScreenshot(out Size size)
        {
            try
            {
                size = Size.Empty;
                WinAPI.RECT rect = new WinAPI.RECT();
                IntPtr hForWnd = WinAPI.GetForegroundWindow();
                if (hForWnd == IntPtr.Zero) 
                    return null;
                WinAPI.GetWindowRect(hForWnd, ref rect);
                IntPtr hwnd = WinAPI.GetDC(IntPtr.Zero);
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

            using (Bitmap screenshotImage = TakeScreenshot(out size))
            {
                if (screenshotImage == null)
                    return null;

                screenshot = new Screenshot(size, screenshotImage);
            }

            return screenshot;
        }
    }
}
