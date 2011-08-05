using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Input
{
    [Flags]
    public enum KeyModifiers : int
    {
        None = 0x00,
        LeftControl = 0x01,
        RightControl = 0x02,
        Control = 0x03,
        LeftAlt = 0x04,
        RightAlt = 0x08,
        Alt = 0x0c,
        LeftShift = 0x10,
        RightShift = 0x20,
        Shift = 0x30,
    }

    public struct KeyData
    {
        public Keys Key;
        public KeyModifiers Modifier;

        public override string ToString()
        {
            return Key.ToString();
        }
    }

    public class KeyboardBuffer: MessageHook
    {
        public bool Enabled { get; set; }

        public bool TranslateMessage { get; set; }

        public Stack<KeyData> KeyData { get; private set; }

        KeyModifiers modifier;

        public int Count { get { return KeyData.Count; } }

        public StringBuilder Text { get; private set; }

        public KeyboardBuffer( IntPtr window )
            : base( window )
        {
            KeyData = new Stack<KeyData>();
            Text = new StringBuilder();
        }
        
        public string GetText()
        {
            string text = Text.ToString();
            Text.Length = 0;
            return text;
        }

        protected override void Hook( ref Message m )
        {
            switch ( m.msg )
            {
            case Wm.KeyDown:
                if ( !Enabled )
                    break;
                //
                KeyData data;
                if ( (m.lparam.ToInt32() & (1 << 30)) == 0 )//iff repeat count == 0
                {
                    switch ( (Vk)m.wparam )
                    {
                    case Vk.Control:
                        if ( (m.lparam.ToInt32() & (1 << 24)) == 0 )
                        {
                            data = new KeyData { Key = Keys.LeftControl, Modifier = modifier };
                            KeyData.Push( ref data );
                            modifier |= KeyModifiers.LeftControl;
                        }
                        else
                        {
                            data = new KeyData { Key = Keys.RightControl, Modifier = modifier };
                            KeyData.Push( ref data );
                            modifier |= KeyModifiers.RightControl;
                        }
                        break;
                    case Vk.Alt:
                        if ( (m.lparam.ToInt32() & (1 << 24)) == 0 )
                        {
                            data = new KeyData { Key = Keys.LeftAlt, Modifier = modifier };
                            KeyData.Push( ref data );
                            modifier |= KeyModifiers.LeftAlt;
                        }
                        else
                        {
                            data = new KeyData { Key = Keys.RightAlt, Modifier = modifier };
                            KeyData.Push( ref data );
                            modifier |= KeyModifiers.RightAlt;
                        }
                        break;
                    case Vk.Shift:
                        if ( (m.lparam.ToInt32() & (1 << 24)) == 0 )
                        {
                            data = new KeyData { Key = Keys.LeftShift, Modifier = modifier };
                            KeyData.Push( ref data );
                            modifier |= KeyModifiers.LeftShift;
                        }
                        else
                        {
                            data = new KeyData { Key = Keys.RightShift, Modifier = modifier };
                            KeyData.Push( ref data );
                            modifier |= KeyModifiers.RightShift;
                        }
                        break;
                    //
                    default:
                        data = new KeyData { Key = (Keys)m.wparam, Modifier = modifier };
                        KeyData.Push( ref data );
                        break;
                    }
                }
                //
                if ( TranslateMessage )
                    _TranslateMessage( ref m );
                //
                break;

            case Wm.Char:
                char c = (char)m.wparam;
                if ( c < (char)0x20 
                    && c != '\n'
                    && c != '\r'
                    //&& c != '\t'//tab //uncomment to accept tab
                    && c != '\b' )//backspace
                    break;
                //
                if ( c == '\r' )
                    c = '\n';//Note: Control+ENTER will send \n, just ENTER will send \r
                //
                if ( c == '\b' && Text.Length > 0 && Text[Text.Length - 1] != '\b' )
                    Text.Length--;//pop 1
                //
                Text.Append( c );
                break;
                
            case Wm.KeyUp:
                switch ( (Vk)m.wparam )
                {
                case Vk.Control:
                    if ( (m.lparam.ToInt32() & (1 << 24)) == 0 )
                        modifier &= ~KeyModifiers.LeftControl;
                    else
                        modifier &= ~KeyModifiers.RightControl;
                    break;
                case Vk.Alt:
                    if ( (m.lparam.ToInt32() & (1 << 24)) == 0 )
                        modifier &= ~KeyModifiers.LeftAlt;
                    else
                        modifier &= ~KeyModifiers.RightAlt;
                    break;
                case Vk.Shift:
                    if ( (m.lparam.ToInt32() & (1 << 24)) == 0 )
                        modifier &= ~KeyModifiers.LeftShift;
                    else
                        modifier &= ~KeyModifiers.RightShift;
                    break;
                }
                break;

            case Wm.Active:
                if ( ((int)m.wparam & 0xffff) == (int)Wa.Inactive )
                {
                    modifier = KeyModifiers.None;
                }
                break;
            }
        }
    }
}
