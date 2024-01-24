using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms; 
namespace D_Clipboards
{

    public class ScreenCapture
    {
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

        private const int MONITOR_DEFAULTTONEAREST = 2;
        private const int MDT_EFFECTIVE_DPI = 0;

        private List<Bitmap> capturedScreens;

        public ScreenCapture()
        {
            capturedScreens = new List<Bitmap>();
        }

        public Bitmap[] CaptureAllScreens()
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                //IntPtr monitorHandle = MonitorFromWindow( IntPtr.Zero, MONITOR_DEFAULTTONEAREST);
                //GetDpiForMonitor(monitorHandle, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);

                //float scaleFactorX = (float)dpiX / 96;
                //float scaleFactorY = (float)dpiY / 96;

                Rectangle screenSize = screen.Bounds;
                Bitmap screenBitmap = new Bitmap((int)(screenSize.Width ), (int)(screenSize.Height ));

                using (Graphics g = Graphics.FromImage(screenBitmap))
                {
                    g.CopyFromScreen(screenSize.X, screenSize.Y, 0, 0, new Size((int)(screenSize.Width  ), (int)(screenSize.Height )));
                }

                capturedScreens.Add(screenBitmap);
               
            }
            return capturedScreens.ToArray();
        }

        public Bitmap[] GetCapturedScreens()
        {
            return capturedScreens.ToArray();
        }
    }
}
