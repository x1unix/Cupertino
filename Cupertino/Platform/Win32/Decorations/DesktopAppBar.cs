﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable EnumUnderlyingTypeIsInt
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace Cupertino.Platform.Win32.Decorations
{
    /// <summary>
    /// 表示窗口停靠到桌面上时的边缘方向。
    /// </summary>
    public enum AppBarEdge
    {
        /// <summary>
        /// 窗口停靠到桌面的左边。
        /// </summary>
        Left = 0,

        /// <summary>
        /// 窗口停靠到桌面的顶部。
        /// </summary>
        Top,

        /// <summary>
        /// 窗口停靠到桌面的右边。
        /// </summary>
        Right,

        /// <summary>
        /// 窗口停靠到桌面的底部。
        /// </summary>
        Bottom,

        /// <summary>
        /// 窗口不停靠到任何方向，而是成为一个普通窗口占用剩余的可用空间（工作区）。
        /// </summary>
        None
    }

    /// <summary>
    /// 提供将窗口停靠到桌面某个方向的能力。
    /// </summary>
    public class DesktopAppBar
    {
        public static readonly DependencyProperty AppBarProperty = DependencyProperty.RegisterAttached(
            "AppBar", typeof(AppBarEdge), typeof(DesktopAppBar),
            new PropertyMetadata(AppBarEdge.None, OnAppBarEdgeChanged));

        public static AppBarEdge GetAppBar(Window window) => (AppBarEdge)window.GetValue(AppBarProperty);

        public static void SetAppBar(Window window, AppBarEdge value) => window.SetValue(AppBarProperty, value);

        private static readonly DependencyProperty AppBarProcessorProperty = DependencyProperty.RegisterAttached(
            "AppBarProcessor", typeof(AppBarWindowProcessor), typeof(DesktopAppBar), new PropertyMetadata(null));

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static void OnAppBarEdgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d))
            {
                return;
            }

            var oldValue = (AppBarEdge)e.OldValue;
            var newValue = (AppBarEdge)e.NewValue;
            var oldEnabled = oldValue is AppBarEdge.Left
                             || oldValue is AppBarEdge.Top
                             || oldValue is AppBarEdge.Right
                             || oldValue is AppBarEdge.Bottom;
            var newEnabled = newValue is AppBarEdge.Left
                             || newValue is AppBarEdge.Top
                             || newValue is AppBarEdge.Right
                             || newValue is AppBarEdge.Bottom;
            if (oldEnabled && !newEnabled)
            {
                var processor = (AppBarWindowProcessor)d.GetValue(AppBarProcessorProperty);
                processor.Detach();
            }
            else if (!oldEnabled && newEnabled)
            {
                var processor = new AppBarWindowProcessor((Window)d);
                d.SetValue(AppBarProcessorProperty, processor);
                processor.Attach(newValue);
            }
            else if (oldEnabled && newEnabled)
            {
                var processor = (AppBarWindowProcessor)d.GetValue(AppBarProcessorProperty);
                processor.Update(newValue);
            }
        }

        /// <summary>
        /// 包含对 <see cref="Window"/> 进行操作以便使其成为一个桌面停靠窗口的能力。
        /// </summary>
        private class AppBarWindowProcessor
        {
            /// <summary>
            /// 创建 <see cref="AppBarWindowProcessor"/> 的新实例。
            /// </summary>
            /// <param name="window">需要成为停靠窗口的 <see cref="Window"/> 的实例。</param>
            public AppBarWindowProcessor(Window window)
            {
                _window = window;
                _callbackId = User32.RegisterWindowMessage("AppBarMessage");
                _hwndSourceTask = new TaskCompletionSource<HwndSource>();

                var source = (HwndSource)PresentationSource.FromVisual(window);
                if (source == null)
                {
                    window.SourceInitialized += OnSourceInitialized;
                }
                else
                {
                    _hwndSourceTask.SetResult(source);
                }

                _window.Closed += OnClosed;
            }

            private readonly Window _window;
            private readonly TaskCompletionSource<HwndSource> _hwndSourceTask;
            private readonly int _callbackId;

            private WindowStyle _restoreStyle;
            private Rect _restoreBounds;
            private ResizeMode _restoreResizeMode;
            private bool _restoreTopmost;

            private AppBarEdge Edge { get; set; }

            /// <summary>
            /// 在可以获取到窗口句柄的时候，给窗口句柄设置值。
            /// </summary>
            private void OnSourceInitialized(object sender, EventArgs e)
            {
                _window.SourceInitialized -= OnSourceInitialized;
                var source = (HwndSource)PresentationSource.FromVisual(_window);
                _hwndSourceTask.SetResult(source);
            }

            /// <summary>
            /// 在窗口关闭之后，需要恢复窗口设置过的停靠属性。
            /// </summary>
            private void OnClosed(object sender, EventArgs e)
            {
                _window.Closed -= OnClosed;
                _window.ClearValue(AppBarProperty);
            }

            /// <summary>
            /// 将窗口属性设置为停靠所需的属性。
            /// </summary>
            private void ForceWindowProperties()
            {
                _window.WindowStyle = WindowStyle.None;
                _window.ResizeMode = ResizeMode.NoResize;
                _window.Topmost = true;
            }

            /// <summary>
            /// 备份窗口在成为停靠窗口之前的属性。
            /// </summary>
            private void BackupWindowProperties()
            {
                _restoreStyle = _window.WindowStyle;
                _restoreBounds = _window.RestoreBounds;
                _restoreResizeMode = _window.ResizeMode;
                _restoreTopmost = _window.Topmost;
            }

            /// <summary>
            /// 使一个窗口开始成为桌面停靠窗口，并开始处理窗口停靠消息。
            /// </summary>
            /// <param name="value">停靠方向。</param>
            public async void Attach(AppBarEdge value)
            {
                var hwndSource = await _hwndSourceTask.Task;

                BackupWindowProperties();

                var data = new User32.APPBARDATA();
                data.cbSize = Marshal.SizeOf(data);
                data.hWnd = hwndSource.Handle;

                data.uCallbackMessage = _callbackId;
                User32.SHAppBarMessage((int)User32.ABMsg.ABM_NEW, ref data);
                hwndSource.AddHook(WndProc);

                Update(value);
            }

            /// <summary>
            /// 更新一个窗口的停靠方向。
            /// </summary>
            /// <param name="value">停靠方向。</param>
            public async void Update(AppBarEdge value)
            {
                var hwndSource = await _hwndSourceTask.Task;

                Edge = value;


                var bounds = TransformToAppBar(hwndSource.Handle, _window.RestoreBounds, value);
                ForceWindowProperties();
                Resize(_window, bounds);
            }

            /// <summary>
            /// 使一个窗口从桌面停靠窗口恢复成普通窗口。
            /// </summary>
            public async void Detach()
            {
                var hwndSource = await _hwndSourceTask.Task;

                var data = new User32.APPBARDATA();
                data.cbSize = Marshal.SizeOf(data);
                data.hWnd = hwndSource.Handle;

                User32.SHAppBarMessage((int)User32.ABMsg.ABM_REMOVE, ref data);

                _window.WindowStyle = _restoreStyle;
                _window.ResizeMode = _restoreResizeMode;
                _window.Topmost = _restoreTopmost;

                Resize(_window, _restoreBounds);
            }

            private IntPtr WndProc(IntPtr hwnd, int msg,
                IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                if (msg == _callbackId)
                {
                    if (wParam.ToInt32() == (int)User32.ABNotify.ABN_POSCHANGED)
                    {
                        var hwndSource = _hwndSourceTask.Task.Result;
                        var bounds = TransformToAppBar(hwndSource.Handle, _window.RestoreBounds, Edge);
                        Resize(_window, bounds);
                        handled = true;
                    }
                }

                return IntPtr.Zero;
            }

            private static void Resize(Window window, Rect bounds)
            {
                window.Left = bounds.Left;
                window.Top = bounds.Top;
                window.Width = bounds.Width;
                window.Height = bounds.Height;
            }

            private Rect TransformToAppBar(IntPtr hWnd, Rect area, AppBarEdge edge)
            {
                var data = new User32.APPBARDATA();
                data.cbSize = Marshal.SizeOf(data);
                data.hWnd = hWnd;
                data.uEdge = (int)edge;

                if (data.uEdge == (int)AppBarEdge.Left || data.uEdge == (int)AppBarEdge.Right)
                {
                    data.rc.top = 0;
                    data.rc.bottom = (int)SystemParameters.PrimaryScreenHeight;
                    if (data.uEdge == (int)AppBarEdge.Left)
                    {
                        data.rc.left = 0;
                        data.rc.right = (int)Math.Round(area.Width);
                    }
                    else
                    {
                        data.rc.right = (int)SystemParameters.PrimaryScreenWidth;
                        data.rc.left = data.rc.right - (int)Math.Round(area.Width);
                    }
                }
                else
                {
                    data.rc.left = 0;
                    data.rc.right = (int)SystemParameters.PrimaryScreenWidth;
                    if (data.uEdge == (int)AppBarEdge.Top)
                    {
                        data.rc.top = 0;
                        data.rc.bottom = (int)Math.Round(area.Height);
                    }
                    else
                    {
                        data.rc.bottom = (int)SystemParameters.PrimaryScreenHeight;
                        data.rc.top = data.rc.bottom - (int)Math.Round(area.Height);
                    }
                }

                User32.SHAppBarMessage((int)User32.ABMsg.ABM_QUERYPOS, ref data);
                User32.SHAppBarMessage((int)User32.ABMsg.ABM_SETPOS, ref data);

                return new Rect(data.rc.left, data.rc.top,
                    data.rc.right - data.rc.left, data.rc.bottom - data.rc.top);
            }

            
        }
    }
}