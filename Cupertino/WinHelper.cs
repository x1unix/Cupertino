using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Cupertino
{
    class WinHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd,
            StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop,
            EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        // Define the callback delegate's type.
        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        private static EnumDelegate CreateFilterCallback(List<IntPtr> winHandles, List<string> winTitles)
        {
            EnumDelegate cb = (IntPtr hWnd, int lParam) =>
            {
                // Get the window's title.
                StringBuilder sb_title = new StringBuilder(1024);
                int length = GetWindowText(hWnd, sb_title, sb_title.Capacity);
                string title = sb_title.ToString();

                // If the window is visible and has a title, save it.
                if (IsWindowVisible(hWnd) &&
                    string.IsNullOrEmpty(title) == false)
                {
                    winHandles.Add(hWnd);
                    winTitles.Add(title);
                }

                // Return true to indicate that we
                // should continue enumerating windows.
                return true;
            };

            return cb;
        }

        // Return a list of the desktop windows' handles and titles.
        public static void GetDesktopWindowHandlesAndTitles(out List<IntPtr> handles, out List<string> titles)
        {
            var WindowHandles = new List<IntPtr>();
            var WindowTitles = new List<string>();
            var cb = WinHelper.CreateFilterCallback(WindowHandles, WindowTitles);
            if (!EnumDesktopWindows(IntPtr.Zero, cb, IntPtr.Zero))
            {
                handles = null;
                titles = null;
            }
            else
            {
                handles = WindowHandles;
                titles = WindowTitles;
            }
        }
    }

}
