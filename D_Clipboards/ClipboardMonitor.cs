using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class ClipboardMonitor : NativeWindow
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    public event EventHandler ClipboardChanged;

    public ClipboardMonitor()
    {
        CreateHandle(new CreateParams());
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_CLIPBOARDUPDATE)
        {
            OnClipboardChanged();
        }

        base.WndProc(ref m);
    }

    private void OnClipboardChanged()
    {
        ClipboardChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartMonitoring()
    {
        AddClipboardFormatListener(Handle);
        Application.Run();
    }

    public void StopMonitoring()
    {
        RemoveClipboardFormatListener(Handle);
        Application.Exit();
    }
}