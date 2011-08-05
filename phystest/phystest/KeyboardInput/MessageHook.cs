using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Input
{
    public abstract class MessageHook: IDisposable
    {
        #region Nested

        protected enum Wm
        {
            Active = 6,
            Char = 0x102,
            KeyDown = 0x100,
            KeyUp = 0x101,
            SysKeyDown = 260,
            SysKeyUp = 0x105
        }

        protected enum Wa
        {
            Inactive,
            Active,
            ClickActive
        }

        protected enum Vk
        {
            Alt = 0x12,
            Control = 0x11,
            Shift = 0x10
        }

        protected struct Message
        {
            IntPtr window;
            //
            public Wm       msg;
            public IntPtr   wparam;
            public IntPtr   lparam;
            //...
        }

        #endregion

        #region Interop

        delegate int MessageHookProc( int code, IntPtr wparam, ref Message lparam );

        [DllImport( "user32.dll" )]
        extern static int GetWindowThreadProcessId( IntPtr window, IntPtr module );

        [DllImport( "user32.dll" )]
        extern static IntPtr SetWindowsHookEx( int type, MessageHookProc hook, IntPtr module, int threadId );

        [DllImport( "user32.dll" )]
        extern static int CallNextHookEx( IntPtr hookfunc, int code, IntPtr wparam, ref Message m );

        [DllImport( "user32.dll" )]
        extern static bool UnhookWindowsHookEx( IntPtr hookfunc );

        [DllImport( "user32.dll", EntryPoint = "TranslateMessage" )]
        protected extern static bool _TranslateMessage( ref Message m );

        #endregion

        MessageHookProc proc;
        IntPtr hookfunc;

        public MessageHook( IntPtr window )
        {
            int threadId = GetWindowThreadProcessId( window, IntPtr.Zero );
            IntPtr hr;
            if ( (hr = SetWindowsHookEx( /*WH_GETMESSAGE*/ 3,
                    (proc = __MessageHookProc), IntPtr.Zero, threadId )) == IntPtr.Zero )
            {
                throw new Win32Exception();
            }
            hookfunc = hr;
        }

        int __MessageHookProc( int code, IntPtr wparam, ref Message m )
        {
            if ( code > -1 && wparam.ToInt32() == /*PM_REMOVE*/ 1 )
            {
                Hook( ref m );
            }
            //
            return CallNextHookEx( hookfunc, code, wparam, ref m );
        }

        protected abstract void Hook( ref Message m );

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( hookfunc != IntPtr.Zero )
                {
                    UnhookWindowsHookEx( hookfunc );
                    hookfunc = IntPtr.Zero;
                }
            }
        }

        #endregion
    }
}
