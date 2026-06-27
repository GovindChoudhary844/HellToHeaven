using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public static class FullscreenGameView
{
    private static EditorWindow fullscreenWindow;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_SHOWWINDOW = 0x0040;

    [MenuItem("Window/Toggle Fullscreen Game View _F11")]
    public static void ToggleFullscreen()
    {
        if (fullscreenWindow != null) 
        {
            CloseFullscreen();
        }
        else 
        {
            OpenFullscreen();
        }
    }

    private static void CloseFullscreen()
    {
        if (fullscreenWindow != null)
        {
            fullscreenWindow.Close();
            fullscreenWindow = null;
        }
    }

    private static void OpenFullscreen()
    {
        Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");

        if (gameViewType == null)
        {
            Debug.LogError("Could not find GameView type.");
            return;
        }

        if (fullscreenWindow != null) return;

        fullscreenWindow = (EditorWindow)ScriptableObject.CreateInstance(gameViewType);
        
        // Hide the GameView toolbar using reflection
        PropertyInfo showToolbarProp = gameViewType.GetProperty("showToolbar", BindingFlags.Instance | BindingFlags.NonPublic);
        if (showToolbarProp != null)
        {
            showToolbarProp.SetValue(fullscreenWindow, false);
        }

        // Show as a borderless popup
        fullscreenWindow.ShowPopup();
        
        Resolution res = Screen.currentResolution;
        fullscreenWindow.position = new Rect(0, 0, res.width, res.height);
        fullscreenWindow.Focus();

        // Ensure the taskbar is covered by setting the window to Topmost via Windows API
#if UNITY_EDITOR_WIN
        IntPtr hwnd = GetActiveWindow();
        if (hwnd != IntPtr.Zero)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, res.width, res.height, SWP_SHOWWINDOW);
        }
#endif
    }
}
