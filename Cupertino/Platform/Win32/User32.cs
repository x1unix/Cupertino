using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Cupertino.Platform.Win32
{
    class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public readonly IntPtr lParam;
        }

        public enum ABMsg : int
        {
            ABM_NEW = 0,
            ABM_REMOVE,
            ABM_QUERYPOS,
            ABM_SETPOS,
            ABM_GETSTATE,
            ABM_GETTASKBARPOS,
            ABM_ACTIVATE,
            ABM_GETAUTOHIDEBAR,
            ABM_SETAUTOHIDEBAR,
            ABM_WINDOWPOSCHANGED,
            ABM_SETSTATE
        }

        public enum ABNotify : int
        {
            ABN_STATECHANGE = 0,
            ABN_POSCHANGED,
            ABN_FULLSCREENAPP,
            ABN_WINDOWARRANGE
        }

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string msg);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(
            uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess,
            uint idThread, uint dwFlags);

        public delegate void WinEventDelegate(
            IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        /// <summary>
        ///     Copies the text of the specified window's title bar (if it has one) into a buffer. If the specified window is a
        ///     control, the text of the control is copied. However, GetWindowText cannot retrieve the text of a control in another
        ///     application.
        ///     <para>
        ///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633520%28v=vs.85%29.aspx  for more
        ///     information
        ///     </para>
        /// </summary>
        /// <param name="hWnd">
        ///     C++ ( hWnd [in]. Type: HWND )<br />A <see cref="IntPtr" /> handle to the window or control containing the text.
        /// </param>
        /// <param name="lpString">
        ///     C++ ( lpString [out]. Type: LPTSTR )<br />The <see cref="StringBuilder" /> buffer that will receive the text. If
        ///     the string is as long or longer than the buffer, the string is truncated and terminated with a null character.
        /// </param>
        /// <param name="nMaxCount">
        ///     C++ ( nMaxCount [in]. Type: int )<br /> Should be equivalent to
        ///     <see cref="StringBuilder.Length" /> after call returns. The <see cref="int" /> maximum number of characters to copy
        ///     to the buffer, including the null character. If the text exceeds this limit, it is truncated.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is the length, in characters, of the copied string, not including
        ///     the terminating null character. If the window has no title bar or text, if the title bar is empty, or if the window
        ///     or control handle is invalid, the return value is zero. To get extended error information, call GetLastError.<br />
        ///     This function cannot retrieve the text of an edit control in another application.
        /// </returns>
        /// <remarks>
        ///     If the target window is owned by the current process, GetWindowText causes a WM_GETTEXT message to be sent to the
        ///     specified window or control. If the target window is owned by another process and has a caption, GetWindowText
        ///     retrieves the window caption text. If the window does not have a caption, the return value is a null string. This
        ///     behavior is by design. It allows applications to call GetWindowText without becoming unresponsive if the process
        ///     that owns the target window is not responding. However, if the target window is not responding and it belongs to
        ///     the calling application, GetWindowText will cause the calling application to become unresponsive. To retrieve the
        ///     text of a control in another process, send a WM_GETTEXT message directly instead of calling GetWindowText.<br />For
        ///     an example go to
        ///     <see cref="!:https://msdn.microsoft.com/en-us/library/windows/desktop/ms644928%28v=vs.85%29.aspx#sending">
        ///     Sending a
        ///     Message.
        ///     </see>
        /// </remarks>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Retrieves a handle to the menu assigned to the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose menu handle is to be retrieved.</param>
        /// <returns>
        /// The return value is a handle to the menu. If the specified window has no menu, the return value is NULL.
        /// If the window is a child window, the return value is undefined.
        /// </returns>
        [DllImport("user32.dll")]
        static extern IntPtr GetMenu(IntPtr hWnd);

        public const UInt32 MF_BYPOSITION = 0x00000400;

        /** Events **/

        public const uint WINEVENT_OUTOFCONTEXT = 0x0000; // Events are ASYNC

        public const uint WINEVENT_SKIPOWNTHREAD = 0x0001; // Don't call back for events on installer's thread

        public const uint WINEVENT_SKIPOWNPROCESS = 0x0002; // Don't call back for events on installer's process

        public const uint WINEVENT_INCONTEXT = 0x0004; // Events are SYNC, this causes your dll to be injected into every process

        public const uint EVENT_MIN = 0x00000001;

        public const uint EVENT_MAX = 0x7FFFFFFF;

        public const uint EVENT_SYSTEM_SOUND = 0x0001;

        public const uint EVENT_SYSTEM_ALERT = 0x0002;

        public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

        public const uint EVENT_SYSTEM_MENUSTART = 0x0004;

        public const uint EVENT_SYSTEM_MENUEND = 0x0005;

        public const uint EVENT_SYSTEM_MENUPOPUPSTART = 0x0006;

        public const uint EVENT_SYSTEM_MENUPOPUPEND = 0x0007;

        public const uint EVENT_SYSTEM_CAPTURESTART = 0x0008;

        public const uint EVENT_SYSTEM_CAPTUREEND = 0x0009;

        public const uint EVENT_SYSTEM_MOVESIZESTART = 0x000A;

        public const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;

        public const uint EVENT_SYSTEM_CONTEXTHELPSTART = 0x000C;

        public const uint EVENT_SYSTEM_CONTEXTHELPEND = 0x000D;

        public const uint EVENT_SYSTEM_DRAGDROPSTART = 0x000E;

        public const uint EVENT_SYSTEM_DRAGDROPEND = 0x000F;

        public const uint EVENT_SYSTEM_DIALOGSTART = 0x0010;

        public const uint EVENT_SYSTEM_DIALOGEND = 0x0011;

        public const uint EVENT_SYSTEM_SCROLLINGSTART = 0x0012;

        public const uint EVENT_SYSTEM_SCROLLINGEND = 0x0013;

        public const uint EVENT_SYSTEM_SWITCHSTART = 0x0014;

        public const uint EVENT_SYSTEM_SWITCHEND = 0x0015;

        public const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;

        public const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;

        public const uint EVENT_SYSTEM_DESKTOPSWITCH = 0x0020;

        public const uint EVENT_SYSTEM_END = 0x00FF;

        public const uint EVENT_OEM_DEFINED_START = 0x0101;

        public const uint EVENT_OEM_DEFINED_END = 0x01FF;

        public const uint EVENT_UIA_EVENTID_START = 0x4E00;

        public const uint EVENT_UIA_EVENTID_END = 0x4EFF;

        public const uint EVENT_UIA_PROPID_START = 0x7500;

        public const uint EVENT_UIA_PROPID_END = 0x75FF;

        public const uint EVENT_CONSOLE_CARET = 0x4001;

        public const uint EVENT_CONSOLE_UPDATE_REGION = 0x4002;

        public const uint EVENT_CONSOLE_UPDATE_SIMPLE = 0x4003;

        public const uint EVENT_CONSOLE_UPDATE_SCROLL = 0x4004;

        public const uint EVENT_CONSOLE_LAYOUT = 0x4005;

        public const uint EVENT_CONSOLE_START_APPLICATION = 0x4006;

        public const uint EVENT_CONSOLE_END_APPLICATION = 0x4007;

        public const uint EVENT_CONSOLE_END = 0x40FF;

        public const uint EVENT_OBJECT_CREATE = 0x8000; // hwnd ID idChild is created item

        public const uint EVENT_OBJECT_DESTROY = 0x8001; // hwnd ID idChild is destroyed item

        public const uint EVENT_OBJECT_SHOW = 0x8002; // hwnd ID idChild is shown item

        public const uint EVENT_OBJECT_HIDE = 0x8003; // hwnd ID idChild is hidden item

        public const uint EVENT_OBJECT_REORDER = 0x8004; // hwnd ID idChild is parent of zordering children

        public const uint EVENT_OBJECT_FOCUS = 0x8005; // hwnd ID idChild is focused item

        public const uint EVENT_OBJECT_SELECTION = 0x8006; // hwnd ID idChild is selected item (if only one), or idChild is OBJID_WINDOW if complex

        public const uint EVENT_OBJECT_SELECTIONADD = 0x8007; // hwnd ID idChild is item added

        public const uint EVENT_OBJECT_SELECTIONREMOVE = 0x8008; // hwnd ID idChild is item removed

        public const uint EVENT_OBJECT_SELECTIONWITHIN = 0x8009; // hwnd ID idChild is parent of changed selected items

        public const uint EVENT_OBJECT_STATECHANGE = 0x800A; // hwnd ID idChild is item w/ state change

        public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B; // hwnd ID idChild is moved/sized item

        public const uint EVENT_OBJECT_NAMECHANGE = 0x800C; // hwnd ID idChild is item w/ name change

        public const uint EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D; // hwnd ID idChild is item w/ desc change

        public const uint EVENT_OBJECT_VALUECHANGE = 0x800E; // hwnd ID idChild is item w/ value change

        public const uint EVENT_OBJECT_PARENTCHANGE = 0x800F; // hwnd ID idChild is item w/ new parent

        public const uint EVENT_OBJECT_HELPCHANGE = 0x8010; // hwnd ID idChild is item w/ help change

        public const uint EVENT_OBJECT_DEFACTIONCHANGE = 0x8011; // hwnd ID idChild is item w/ def action change

        public const uint EVENT_OBJECT_ACCELERATORCHANGE = 0x8012; // hwnd ID idChild is item w/ keybd accel change

        public const uint EVENT_OBJECT_INVOKED = 0x8013; // hwnd ID idChild is item invoked

        public const uint EVENT_OBJECT_TEXTSELECTIONCHANGED = 0x8014; // hwnd ID idChild is item w? test selection change

        public const uint EVENT_OBJECT_CONTENTSCROLLED = 0x8015;

        public const uint EVENT_SYSTEM_ARRANGMENTPREVIEW = 0x8016;

        public const uint EVENT_OBJECT_END = 0x80FF;

        public const uint EVENT_AIA_START = 0xA000;

        public const uint EVENT_AIA_END = 0xAFFF;
    }
}
