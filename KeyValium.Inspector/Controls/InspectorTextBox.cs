using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyValium.Inspector.Controls
{
    internal class InspectorTextBox : RichTextBox
    {
        public InspectorTextBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.UserPaint, true);
        }

        private const int WM_USER = 0x0400;
        private const int EM_GETEVENTMASK = WM_USER + 59;
        private const int EM_SETEVENTMASK = WM_USER + 69;
        private const int WM_SETREDRAW = 0x0b;
        private IntPtr OldEventMask;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public void BeginUpdate()
        {
            SendMessage(Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            OldEventMask = SendMessage(Handle, EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero);
        }

        public void EndUpdate()
        {
            SendMessage(Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            SendMessage(Handle, EM_SETEVENTMASK, IntPtr.Zero, OldEventMask);
        }

        private IntPtr _eventmask;

        public void StopRepaint()
        {
            // Stop redrawing:
            SendMessage(Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            // Stop sending of events:
            _eventmask = SendMessage(Handle, EM_GETEVENTMASK, IntPtr.Zero, IntPtr.Zero);
        }

        public void StartRepaint()
        {
            // turn on events
            SendMessage(Handle, EM_SETEVENTMASK, IntPtr.Zero, _eventmask);
            // turn on redrawing
            SendMessage(Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            // this forces a repaint, which for some reason is necessary in some cases.
            Invalidate();
        }
    }
}
