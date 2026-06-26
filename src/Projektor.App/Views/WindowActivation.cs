using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Controls;

namespace Projektor.App.Views;

/// <summary>
/// Forces a window to the foreground with keyboard focus. A tray/background process on Windows
/// cannot simply call SetForegroundWindow (foreground lock), so we temporarily attach our input
/// queue to the current foreground thread — the standard AttachThreadInput workaround.
/// No-op on non-Windows platforms (X11/Wayland handle this via Activate()).
/// </summary>
internal static class WindowActivation
{
    public static void ForceForeground(Window window)
    {
        if (!OperatingSystem.IsWindows())
            return;

        var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle != IntPtr.Zero)
            ForceForegroundWindows(handle);
    }

    [SupportedOSPlatform("windows")]
    private static void ForceForegroundWindows(IntPtr hWnd)
    {
        var foreground = GetForegroundWindow();
        var foreThread = GetWindowThreadProcessId(foreground, out _);
        var appThread = GetCurrentThreadId();

        if (foreThread != appThread && foreground != IntPtr.Zero)
        {
            AttachThreadInput(foreThread, appThread, true);
            try
            {
                BringWindowToTop(hWnd);
                ShowWindow(hWnd, SW_SHOW);
                SetForegroundWindow(hWnd);
                SetFocus(hWnd);
            }
            finally
            {
                AttachThreadInput(foreThread, appThread, false);
            }
        }
        else
        {
            BringWindowToTop(hWnd);
            SetForegroundWindow(hWnd);
            SetFocus(hWnd);
        }
    }

    private const int SW_SHOW = 5;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hWnd);
}
