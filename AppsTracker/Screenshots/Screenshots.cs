using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using Task_Logger_Pro.Logging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task_Logger_Pro.Controls;
using AppsTracker.Models.EntityModels;

namespace Task_Logger_Pro
{
    internal static class Screenshots
    {

        private static Bitmap TakeScreenShot(out Size size)
        {
            try
            {
                size = Size.Empty;
                WinAPI.RECT rect = new WinAPI.RECT();
                IntPtr hForWnd = WinAPI.GetForegroundWindow();
                if (hForWnd == IntPtr.Zero) return null;
                WinAPI.GetWindowRect(hForWnd, ref rect);
                IntPtr hwnd = WinAPI.GetDC(IntPtr.Zero);
                if (hwnd == IntPtr.Zero) return null;
                using (Graphics g = Graphics.FromHdc(hwnd))
                {
                    Bitmap tempImg = new Bitmap(rect.Width, rect.Height, g);
                    using (Graphics tempG = Graphics.FromImage(tempImg))
                    {
                        tempG.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
                    }
                    size = new Size(rect.Width, rect.Height);
                    return tempImg;
                }
            }
            catch (Exception)
            {
                size = Size.Empty;
                return null;
            }

        }

        internal static Screenshot GetScreenshot()
        {
            Size size;
            Screenshot screenshot;

            using (Bitmap screenshotImage = TakeScreenShot(out size))
            {
                if (screenshotImage == null)
                    return null;
                screenshot = new Screenshot(size, screenshotImage);
            }

            return screenshot;
        }

        public static void SaveScreenshotToFileAsync(Screenshot screenshot, string path)
        {
            if (screenshot == null || screenshot.Screensht == null)
                throw new ArgumentNullException();
            //return Task.Run(() =>
            //{
                SaveScreenShot(screenshot.Screensht, path);
            //});
        }

        //public static Image ByteArrayToImage(byte[] byteArrayIn)
        //{
        //    MemoryStream ms = new MemoryStream(byteArrayIn);
        //    Image returnImage = Image.FromStream(ms);
        //    return returnImage;
        //}

        public static string CorrectPath(string windowTitle)
        {
            string newTitle = windowTitle;
            char[] illegalChars = new char[] { '<', '>', ':', '"', '\\', '|', '?', '*', '0' };
            if (windowTitle.IndexOfAny(illegalChars) >= 0)
            {
                foreach (var chr in illegalChars)
                {
                    if (newTitle.Contains(chr))
                    {
                        while (newTitle.Contains(chr))
                        {
                            newTitle = newTitle.Remove(newTitle.IndexOf(chr), 1);
                        }
                    }
                }
            }
            char[] charArray = newTitle.ToArray();
            foreach (var chr in charArray)
            {
                int i = chr;
                if (i >= 1 && i <= 31) newTitle = newTitle.Remove(newTitle.IndexOf(chr), 1);
            }

            return newTitle;
        }

        public static string TrimPath(string path)
        {
            if (path.Length >= 247)
            {
                while (path.Length >= 247)
                {
                    path = path.Remove(path.Length - 1, 1);
                }
            }
            return path;
        }

        private static void SaveScreenShot(byte[] image, string path)
        {
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

            try
            {
                using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate))
                {
                    fileStream.Write(image, 0, image.Length);
                }
            }
            catch (IOException ex)
            {
                MessageWindow window = new MessageWindow(ex);
                window.ShowDialog();
            }

        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
