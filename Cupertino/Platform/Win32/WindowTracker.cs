using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cupertino.Core;
using System.Runtime.InteropServices;
using static PInvoke.User32;
using static PInvoke.Kernel32;
using System.ComponentModel;
using System.Reflection.Emit;

namespace Cupertino.Platform.Win32
{
    public class WindowTracker : IWindowTracker
    {
        private SafeEventHookHandle hook;
        private readonly User32.WinEventDelegate callback;
        private readonly ACCESS_MASK flags = ProcessAccess.PROCESS_QUERY_LIMITED_INFORMATION;
        private Dictionary<string, string> appNameCache = new Dictionary<string, string>();
        //private Dictionary<IntPtr, WinPIDPair> winPidPair = new Dictionary<IntPtr, WinPIDPair>(); 
        //private Dictionary<WinPIDPair, string> winProcessCache = new Dictionary<WinPIDPair, string>();

        public event IWindowTracker.WindowSelectedHandler WindowSelected;

        public WindowTracker() {
            callback = new User32.WinEventDelegate(HandleWinEvent);
        }

        //private struct WinPIDPair
        //{
        //    IntPtr hwnd;
        //    uint pid;
        //}

        public class Win32WindowRef : IWindowRef
        {
            public IntPtr Handle { get; set; }
            public int ProcessID { get; set; }
            public string ExecutablePath { get; set; }
            public string Title { get; set;  }
            public string ApplicationName { get; set; }
            public IWindowRef.MenuRef Menu { get; set; }
        }

        private void HandleWinEvent(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            Task.Run(() =>
            {
                try
                {
                    string winTitle = GetWindowText(hwnd);
                    int processId = 0;
                    GetWindowThreadProcessId(hwnd, out processId);

                    //var flags = Kernel32.ProcessAccessFlags.QueryInformation | Kernel32.ProcessAccessFlags.VirtualMemoryRead; 
                    string executablePath;
                    using (var hProc = OpenProcess(flags, false, processId))
                    {
                        executablePath = QueryFullProcessImageName(hProc, 0);
                    };

                   if (!appNameCache.ContainsKey(executablePath))
                    {
                        var info = FileVersionInfo.GetVersionInfo(executablePath);
                        appNameCache.Add(executablePath, info.FileDescription);
                    }

                    string appName = appNameCache[executablePath];
                    var ev = new Win32WindowRef 
                    {
                        Handle = hwnd,
                        ProcessID = processId,
                        ExecutablePath = executablePath,
                        Title = winTitle.ToString(),
                        ApplicationName = appName
                    };


                    IntPtr hMenu = GetMenu(hwnd);
                    if (hMenu != null)
                    {
                        ev.Menu = new IWindowRef.MenuRef
                        {
                            Ref = hMenu,
                            Items = GetMenuItems(hMenu),
                        };
                    }
                    WindowSelected?.Invoke(ev);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Fuck: {0}", ex);
                }
            });
        }

        private unsafe List<IWindowRef.MenuItemRef> GetMenuItems(IntPtr hMenu) {
            return GetMenuItems(hMenu, IntPtr.Zero);
        }

        private unsafe List<IWindowRef.MenuItemRef> GetMenuItems(IntPtr hMenu, IntPtr rootHandle)
        {
            // ref: https://stackoverflow.com/questions/18589385/retrieve-list-of-menu-items-in-windows-in-c
            // ref: https://stackoverflow.com/questions/22852461/win32-api-getmenuiteminfo-returns-only-the-first-character-of-the-item-text
            int nCount = GetMenuItemCount(hMenu);
            if (nCount <= 0)
            {
                return null;
            }

            MenuMembersMask fMask = MenuMembersMask.MIIM_SUBMENU | MenuMembersMask.MIIM_FTYPE | MenuMembersMask.MIIM_ID | MenuMembersMask.MIIM_STRING;
            List<IWindowRef.MenuItemRef> list = new List<IWindowRef.MenuItemRef>();
            MENUITEMINFO mii = new MENUITEMINFO();

            for (uint i = 0; i < nCount; i++)
            {
                MENUITEMINFO mif = mii.Create();
                mif.dwTypeData = null;
                mif.fMask = fMask;
                if (!GetMenuItemInfo(hMenu, i, true, ref mif))
                {
                    throw new Win32Exception();
                }

                switch (mif.fType)
                {
                    case MenuItemType.MFT_SEPARATOR:
                        list.Add(new IWindowRef.MenuItemRef { Index = i, IsSeparator = true });
                        break;
                    case MenuItemType.MFT_STRING:
                        if (mif.cch <= 0) break;
                        IWindowRef.MenuItemRef item = new IWindowRef.MenuItemRef 
                        { 
                            RootMenuHandle = rootHandle == IntPtr.Zero ? hMenu : rootHandle,
                            MenuHandle = hMenu,
                            Ref = mif.wID, 
                            Index = i, 
                            IsSeparator = false 
                        };
                        mif.cch++;
                        IntPtr szString = Marshal.AllocHGlobal((IntPtr)(sizeof(char) * mif.cch));
                        //IntPtr szString = Marshal.StringToHGlobalUni(new string('\0', mif.cch));
                        mif.dwTypeData = (char*)szString.ToPointer();
                        try
                        {
                            if (!GetMenuItemInfo(hMenu, i, true, ref mif))
                            {
                                throw new Win32Exception();
                            }
                            item.Label = Marshal.PtrToStringUni(szString);
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(szString);
                        }

                        if (mif.hSubMenu != null)
                        {
                            item.SubMenu = new IWindowRef.MenuRef 
                            { 
                                Ref = mif.hSubMenu, 
                                Items = GetMenuItems(mif.hSubMenu, rootHandle) 
                            };
                        }
                        list.Add(item);
                        break;
                    default:
                        break;
                }
            }

            return list;
        }

        public void Attatch()
        {
            IntPtr cbPtr = Marshal.GetFunctionPointerForDelegate(callback);
            hook = SetWinEventHook(WindowsEventHookType.EVENT_SYSTEM_FOREGROUND,
                WindowsEventHookType.EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero, cbPtr, 0, 0, WindowsEventHookFlags.WINEVENT_OUTOFCONTEXT | WindowsEventHookFlags.WINEVENT_SKIPOWNPROCESS);
            //hook = User32.SetWinEventHook(User32.EVENT_SYSTEM_FOREGROUND, User32.EVENT_SYSTEM_FOREGROUND,
            //    IntPtr.Zero, callback, 0, 0, User32.WINEVENT_OUTOFCONTEXT | User32.WINEVENT_SKIPOWNPROCESS);
            Debug.WriteLine("WinEventHook installed");
        }

        public void Dispose()
        {
            if (hook != null)
            {
                Debug.WriteLine("WinEventHook removed");
                User32.UnhookWinEvent(hook.DangerousGetHandle());
            }
        }
    }
}
