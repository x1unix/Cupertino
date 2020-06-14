using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using System.Diagnostics;

namespace Cupertino.Platform.Win32.Decorations
{
    public class AeroDecorator
    {
        private Window window;

        public AeroDecorator(Window w)
        {
            w.Loaded += (object sender, RoutedEventArgs e) => DecorateWindow();
            window = w;
        }

        public void DecorateWindow()
        {
            try
            {
                // Obtain the window handle for WPF application
                IntPtr mainWindowPtr = new WindowInteropHelper(window).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                // Get System Dpi
                System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                float DesktopDpiX = desktop.DpiX;
                float DesktopDpiY = desktop.DpiY;

                // Set Margins
                Dwm.MARGINS margins = new Dwm.MARGINS();

                // Extend glass frame into client area
                // Note that the default desktop Dpi is 96dpi. The  margins are
                // adjusted for the system Dpi.
                margins.cxLeftWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
                margins.cxRightWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
                //margins.cyTopHeight = Convert.ToInt32(((int) topBar.ActualHeight + 5) * (DesktopDpiX / 96));
                margins.cyTopHeight = Convert.ToInt32((32 + 5) * (DesktopDpiX / 96));
                margins.cyBottomHeight = Convert.ToInt32(5 * (DesktopDpiX / 96));

                int hr = Dwm.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                //
                if (hr < 0)
                {
                    Debug.WriteLine("DwmExtendFrameIntoClientArea Failed");
                    //DwmExtendFrameIntoClientArea Failed
                }
            }
            // If not Vista, paint background white.
            catch (DllNotFoundException)
            {
                Application.Current.MainWindow.Background = Brushes.White;
            }
        }
    }
}
