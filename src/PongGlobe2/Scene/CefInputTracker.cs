using System;
using System.Collections.Generic;
using System.Text;
using Xilium.CefGlue;
using Veldrid.Sdl2;
using System.Runtime.CompilerServices;
namespace PongGlobe.Scene
{
    /// <summary>
    /// 为cef设计的InputTracker，直接将sldevents转换为各种SendMessage
    /// </summary>
    public static class CefInputTracker
    {
        enum KeyboardCode
        {
            VKEY_BACK = 0x08,
            VKEY_TAB = 0x09,
            VKEY_BACKTAB = 0x0A,
            VKEY_CLEAR = 0x0C,
            VKEY_RETURN = 0x0D,
            VKEY_SHIFT = 0x10,
            VKEY_CONTROL = 0x11,
            VKEY_MENU = 0x12,
            VKEY_PAUSE = 0x13,
            VKEY_CAPITAL = 0x14,
            VKEY_KANA = 0x15,
            VKEY_HANGUL = 0x15,
            VKEY_JUNJA = 0x17,
            VKEY_FINAL = 0x18,
            VKEY_HANJA = 0x19,
            VKEY_KANJI = 0x19,
            VKEY_ESCAPE = 0x1B,
            VKEY_CONVERT = 0x1C,
            VKEY_NONCONVERT = 0x1D,
            VKEY_ACCEPT = 0x1E,
            VKEY_MODECHANGE = 0x1F,
            VKEY_SPACE = 0x20,
            VKEY_PRIOR = 0x21,
            VKEY_NEXT = 0x22,
            VKEY_END = 0x23,
            VKEY_HOME = 0x24,
            VKEY_LEFT = 0x25,
            VKEY_UP = 0x26,
            VKEY_RIGHT = 0x27,
            VKEY_DOWN = 0x28,
            VKEY_SELECT = 0x29,
            VKEY_PRINT = 0x2A,
            VKEY_EXECUTE = 0x2B,
            VKEY_SNAPSHOT = 0x2C,
            VKEY_INSERT = 0x2D,
            VKEY_DELETE = 0x2E,
            VKEY_HELP = 0x2F,
            VKEY_0 = 0x30,
            VKEY_1 = 0x31,
            VKEY_2 = 0x32,
            VKEY_3 = 0x33,
            VKEY_4 = 0x34,
            VKEY_5 = 0x35,
            VKEY_6 = 0x36,
            VKEY_7 = 0x37,
            VKEY_8 = 0x38,
            VKEY_9 = 0x39,
            VKEY_A = 0x41,
            VKEY_B = 0x42,
            VKEY_C = 0x43,
            VKEY_D = 0x44,
            VKEY_E = 0x45,
            VKEY_F = 0x46,
            VKEY_G = 0x47,
            VKEY_H = 0x48,
            VKEY_I = 0x49,
            VKEY_J = 0x4A,
            VKEY_K = 0x4B,
            VKEY_L = 0x4C,
            VKEY_M = 0x4D,
            VKEY_N = 0x4E,
            VKEY_O = 0x4F,
            VKEY_P = 0x50,
            VKEY_Q = 0x51,
            VKEY_R = 0x52,
            VKEY_S = 0x53,
            VKEY_T = 0x54,
            VKEY_U = 0x55,
            VKEY_V = 0x56,
            VKEY_W = 0x57,
            VKEY_X = 0x58,
            VKEY_Y = 0x59,
            VKEY_Z = 0x5A,
            VKEY_LWIN = 0x5B,
            VKEY_COMMAND = VKEY_LWIN,  // Provide the Mac name for convenience.
            VKEY_RWIN = 0x5C,
            VKEY_APPS = 0x5D,
            VKEY_SLEEP = 0x5F,
            VKEY_NUMPAD0 = 0x60,
            VKEY_NUMPAD1 = 0x61,
            VKEY_NUMPAD2 = 0x62,
            VKEY_NUMPAD3 = 0x63,
            VKEY_NUMPAD4 = 0x64,
            VKEY_NUMPAD5 = 0x65,
            VKEY_NUMPAD6 = 0x66,
            VKEY_NUMPAD7 = 0x67,
            VKEY_NUMPAD8 = 0x68,
            VKEY_NUMPAD9 = 0x69,
            VKEY_MULTIPLY = 0x6A,
            VKEY_ADD = 0x6B,
            VKEY_SEPARATOR = 0x6C,
            VKEY_SUBTRACT = 0x6D,
            VKEY_DECIMAL = 0x6E,
            VKEY_DIVIDE = 0x6F,
            VKEY_F1 = 0x70,
            VKEY_F2 = 0x71,
            VKEY_F3 = 0x72,
            VKEY_F4 = 0x73,
            VKEY_F5 = 0x74,
            VKEY_F6 = 0x75,
            VKEY_F7 = 0x76,
            VKEY_F8 = 0x77,
            VKEY_F9 = 0x78,
            VKEY_F10 = 0x79,
            VKEY_F11 = 0x7A,
            VKEY_F12 = 0x7B,
            VKEY_F13 = 0x7C,
            VKEY_F14 = 0x7D,
            VKEY_F15 = 0x7E,
            VKEY_F16 = 0x7F,
            VKEY_F17 = 0x80,
            VKEY_F18 = 0x81,
            VKEY_F19 = 0x82,
            VKEY_F20 = 0x83,
            VKEY_F21 = 0x84,
            VKEY_F22 = 0x85,
            VKEY_F23 = 0x86,
            VKEY_F24 = 0x87,
            VKEY_NUMLOCK = 0x90,
            VKEY_SCROLL = 0x91,
            VKEY_LSHIFT = 0xA0,
            VKEY_RSHIFT = 0xA1,
            VKEY_LCONTROL = 0xA2,
            VKEY_RCONTROL = 0xA3,
            VKEY_LMENU = 0xA4,
            VKEY_RMENU = 0xA5,
            VKEY_BROWSER_BACK = 0xA6,
            VKEY_BROWSER_FORWARD = 0xA7,
            VKEY_BROWSER_REFRESH = 0xA8,
            VKEY_BROWSER_STOP = 0xA9,
            VKEY_BROWSER_SEARCH = 0xAA,
            VKEY_BROWSER_FAVORITES = 0xAB,
            VKEY_BROWSER_HOME = 0xAC,
            VKEY_VOLUME_MUTE = 0xAD,
            VKEY_VOLUME_DOWN = 0xAE,
            VKEY_VOLUME_UP = 0xAF,
            VKEY_MEDIA_NEXT_TRACK = 0xB0,
            VKEY_MEDIA_PREV_TRACK = 0xB1,
            VKEY_MEDIA_STOP = 0xB2,
            VKEY_MEDIA_PLAY_PAUSE = 0xB3,
            VKEY_MEDIA_LAUNCH_MAIL = 0xB4,
            VKEY_MEDIA_LAUNCH_MEDIA_SELECT = 0xB5,
            VKEY_MEDIA_LAUNCH_APP1 = 0xB6,
            VKEY_MEDIA_LAUNCH_APP2 = 0xB7,
            VKEY_OEM_1 = 0xBA,
            VKEY_OEM_PLUS = 0xBB,
            VKEY_OEM_COMMA = 0xBC,
            VKEY_OEM_MINUS = 0xBD,
            VKEY_OEM_PERIOD = 0xBE,
            VKEY_OEM_2 = 0xBF,
            VKEY_OEM_3 = 0xC0,
            VKEY_OEM_4 = 0xDB,
            VKEY_OEM_5 = 0xDC,
            VKEY_OEM_6 = 0xDD,
            VKEY_OEM_7 = 0xDE,
            VKEY_OEM_8 = 0xDF,
            VKEY_OEM_102 = 0xE2,
            VKEY_OEM_103 = 0xE3,  // GTV KEYCODE_MEDIA_REWIND
            VKEY_OEM_104 = 0xE4,  // GTV KEYCODE_MEDIA_FAST_FORWARD
            VKEY_PROCESSKEY = 0xE5,
            VKEY_PACKET = 0xE7,
            VKEY_DBE_SBCSCHAR = 0xF3,
            VKEY_DBE_DBCSCHAR = 0xF4,
            VKEY_ATTN = 0xF6,
            VKEY_CRSEL = 0xF7,
            VKEY_EXSEL = 0xF8,
            VKEY_EREOF = 0xF9,
            VKEY_PLAY = 0xFA,
            VKEY_ZOOM = 0xFB,
            VKEY_NONAME = 0xFC,
            VKEY_PA1 = 0xFD,
            VKEY_OEM_CLEAR = 0xFE,
            VKEY_UNKNOWN = 0,

            // POSIX specific VKEYs. Note that as of Windows SDK 7.1, 0x97-9F, 0xD8-DA,
            // and 0xE8 are unassigned.
            VKEY_WLAN = 0x97,
            VKEY_POWER = 0x98,
            VKEY_BRIGHTNESS_DOWN = 0xD8,
            VKEY_BRIGHTNESS_UP = 0xD9,
            VKEY_KBD_BRIGHTNESS_DOWN = 0xDA,
            VKEY_KBD_BRIGHTNESS_UP = 0xE8,

            // Windows does not have a specific key code for AltGr. We use the unused 0xE1
            // (VK_OEM_AX) code to represent AltGr, matching the behaviour of Firefox on
            // Linux.
            VKEY_ALTGR = 0xE1,
            // Windows does not have a specific key code for Compose. We use the unused
            // 0xE6 (VK_ICO_CLEAR) code to represent Compose.
            VKEY_COMPOSE = 0xE6,
        };


        private static CefBrowserHost _host;

        public static CefBrowserHost Host { get { return _host; }set { _host = value; } }

        /// <summary>
        /// 直接转换相关事件
        /// </summary>
        /// <param name="ev"></param>
        public static unsafe void HandleEvent(SDL_Event* ev)
        {
            if (_host == null) return;
            switch (ev->type)
            {
                case SDL_EventType.Quit:
                    //Close();
                    break;
                case SDL_EventType.Terminating:
                    //Close();
                    break;
                case SDL_EventType.WindowEvent:
                    //SDL_WindowEvent windowEvent = Unsafe.Read<SDL_WindowEvent>(ev);
                    //HandleWindowEvent(windowEvent);
                    break;
                case SDL_EventType.KeyDown:
                case SDL_EventType.KeyUp:
                    SDL_KeyboardEvent keyboardEvent = Unsafe.Read<SDL_KeyboardEvent>(ev);                   
                    HandleKeyboardEvent(keyboardEvent);
                    break;
                case SDL_EventType.TextEditing:
                    break;
                case SDL_EventType.TextInput:
                    SDL_TextInputEvent textInputEvent = Unsafe.Read<SDL_TextInputEvent>(ev);
                    HandleTextInputEvent(textInputEvent);
                    break;
                case SDL_EventType.KeyMapChanged:
                    break;
                case SDL_EventType.MouseMotion:
                    SDL_MouseMotionEvent mouseMotionEvent = Unsafe.Read<SDL_MouseMotionEvent>(ev);
                    HandleMouseMotionEvent(mouseMotionEvent);
                    break;
                case SDL_EventType.MouseButtonDown:
                case SDL_EventType.MouseButtonUp:
                    SDL_MouseButtonEvent mouseButtonEvent = Unsafe.Read<SDL_MouseButtonEvent>(ev);
                    HandleMouseButtonEvent(mouseButtonEvent);
                    break;
                case SDL_EventType.MouseWheel:
                    SDL_MouseWheelEvent mouseWheelEvent = Unsafe.Read<SDL_MouseWheelEvent>(ev);
                    HandleMouseWheelEvent(mouseWheelEvent);
                    break;
                case SDL_EventType.JoyAxisMotion:
                    break;
                case SDL_EventType.JoyBallMotion:
                    break;
                case SDL_EventType.JoyHatMotion:
                    break;
                case SDL_EventType.JoyButtonDown:
                    break;
                case SDL_EventType.JoyButtonUp:
                    break;
                case SDL_EventType.JoyDeviceAdded:
                    break;
                case SDL_EventType.JoyDeviceRemoved:
                    break;
                case SDL_EventType.ControllerAxisMotion:
                    break;
                case SDL_EventType.ControllerButtonDown:
                    break;
                case SDL_EventType.ControllerButtonUp:
                    break;
                case SDL_EventType.ControllerDeviceAdded:
                    break;
                case SDL_EventType.ControllerDeviceRemoved:
                    break;
                case SDL_EventType.ControllerDeviceRemapped:
                    break;
                case SDL_EventType.FingerDown:
                    break;
                case SDL_EventType.FingerUp:
                    break;
                case SDL_EventType.FingerMotion:
                    break;
                case SDL_EventType.DollarGesture:
                    break;
                case SDL_EventType.DollarRecord:
                    break;
                case SDL_EventType.MultiGesture:
                    break;
                case SDL_EventType.ClipboardUpdate:
                    break;
                case SDL_EventType.DropFile:
                    break;
                case SDL_EventType.DropTest:
                    break;
                case SDL_EventType.DropBegin:
                    break;
                case SDL_EventType.DropComplete:
                    break;
                case SDL_EventType.AudioDeviceAdded:
                    break;
                case SDL_EventType.AudioDeviceRemoved:
                    break;
                case SDL_EventType.RenderTargetsReset:
                    break;
                case SDL_EventType.RenderDeviceReset:
                    break;
                case SDL_EventType.UserEvent:
                    break;
                case SDL_EventType.LastEvent:
                    break;
                default:
                    // Ignore
                    break;
            }
        }

        //处理滚轮事件,暂不知如何处理
        private static void HandleMouseWheelEvent(SDL_MouseWheelEvent e)
        {
            //CefMouseEvent mouseEvent = new CefMouseEvent();
           // mouseEvent.X = e.x;
            //mouseEvent.Y = e.y;           
           // _host.SendMouseWheelEvent(mouseEvent,e.d)
        }



        /// <summary>
        /// 处理TextInput事件
        /// </summary>
        private static unsafe void HandleTextInputEvent(SDL_TextInputEvent e)
        {
            uint byteCount = 0;
            // Loop until the null terminator is found or the max size is reached.
            while (byteCount < SDL_TextInputEvent.MaxTextSize && e.text[byteCount++] != 0)
            { }
            if (byteCount > 1)
            {
                // We don't want the null terminator.
                byteCount -= 1;
                int charCount = Encoding.UTF8.GetCharCount(e.text, (int)byteCount);
                char* charsPtr = stackalloc char[charCount];
                var syt= Encoding.UTF8.GetString(e.text, (int)byteCount);
                string input_text = syt;
                CefRange range = new CefRange();
                range.From = -1;
                _host.ImeCommitText(input_text, range, 0);
            }        
        }



        /// <summary>
        /// 同时处理keydown和up事件
        /// </summary>
        /// <param name="keyboardEvent"></param>
        private static void HandleKeyboardEvent(SDL_KeyboardEvent keyboardEvent)
        {
            
            //SimpleInputSnapshot snapshot = _privateSnapshot;
            CefKeyEvent keyEvent = new CefKeyEvent();
            keyEvent.Modifiers = GetModifiersFromSDL(keyboardEvent.keysym.mod);
            keyEvent.WindowsKeyCode = (int)(GetWindowsKeycodeFromSDLKeycode(keyboardEvent.keysym.scancode));
            keyEvent.NativeKeyCode =(int) keyboardEvent.keysym.scancode;

            keyEvent.EventType = keyboardEvent.state == 0 ? CefKeyEventType.RawKeyDown:CefKeyEventType.KeyUp;
            _host.SendKeyEvent(keyEvent);
        }

        /// <summary>
        /// 获取windowsKeyCode
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static KeyboardCode GetWindowsKeycodeFromSDLKeycode(SDL_Scancode code)
        {
            if (code >= SDL_Scancode.SDL_SCANCODE_1 && code <= SDL_Scancode.SDL_SCANCODE_9)
            {
                return KeyboardCode.VKEY_1 + (code - SDL_Scancode.SDL_SCANCODE_1);
            }
            else if (code >= SDL_Scancode.SDL_SCANCODE_A && code <= SDL_Scancode.SDL_SCANCODE_Z)
            {
                return KeyboardCode.VKEY_A + (code - SDL_Scancode.SDL_SCANCODE_A);
            }
            else if (code >= SDL_Scancode.SDL_SCANCODE_F1 && code <= SDL_Scancode.SDL_SCANCODE_F24)
            {
                return KeyboardCode.VKEY_F1 + (code - SDL_Scancode.SDL_SCANCODE_F1);
            }

            switch (code)
            {
                case SDL_Scancode.SDL_SCANCODE_0:
                    return KeyboardCode.VKEY_9;
                case SDL_Scancode.SDL_SCANCODE_LSHIFT:
                case SDL_Scancode.SDL_SCANCODE_RSHIFT:
                    return KeyboardCode.VKEY_CONTROL;
                case SDL_Scancode.SDL_SCANCODE_LCTRL:
                case SDL_Scancode.SDL_SCANCODE_RCTRL:
                    return KeyboardCode.VKEY_CONTROL;
                case SDL_Scancode.SDL_SCANCODE_LALT:
                case SDL_Scancode.SDL_SCANCODE_RALT:
                    return KeyboardCode.VKEY_ALTGR;
                case SDL_Scancode.SDL_SCANCODE_BACKSPACE:
                    return KeyboardCode.VKEY_BACK;
                case SDL_Scancode.SDL_SCANCODE_RETURN:
                    return KeyboardCode.VKEY_RETURN;
                case SDL_Scancode.SDL_SCANCODE_HOME:
                    return KeyboardCode.VKEY_HOME;
                case SDL_Scancode.SDL_SCANCODE_INSERT:
                    return KeyboardCode.VKEY_INSERT;
                case SDL_Scancode.SDL_SCANCODE_END:
                    return KeyboardCode.VKEY_END;
                case SDL_Scancode.SDL_SCANCODE_SCROLLLOCK:
                    return KeyboardCode.VKEY_SCROLL;
                case SDL_Scancode.SDL_SCANCODE_DELETE:
                    return KeyboardCode.VKEY_DELETE;
                case SDL_Scancode.SDL_SCANCODE_TAB:
                    return KeyboardCode.VKEY_TAB;
                case SDL_Scancode.SDL_SCANCODE_SPACE:
                    return KeyboardCode.VKEY_SPACE;
                case SDL_Scancode.SDL_SCANCODE_COMMA:
                    return KeyboardCode.VKEY_COMMAND;
                case SDL_Scancode.SDL_SCANCODE_LGUI:
                    return KeyboardCode.VKEY_LWIN;
                case SDL_Scancode.SDL_SCANCODE_CAPSLOCK:
                    return KeyboardCode.VKEY_CAPITAL;
                case SDL_Scancode.SDL_SCANCODE_UP:
                    return KeyboardCode.VKEY_UP;
                case SDL_Scancode.SDL_SCANCODE_DOWN:
                    return KeyboardCode.VKEY_DOWN;
                case SDL_Scancode.SDL_SCANCODE_LEFT:
                    return KeyboardCode.VKEY_LEFT;
                case SDL_Scancode.SDL_SCANCODE_RIGHT:
                    return KeyboardCode.VKEY_RIGHT;
                case SDL_Scancode.SDL_SCANCODE_PAGEUP:
                    return KeyboardCode.VKEY_PRIOR;
                case SDL_Scancode.SDL_SCANCODE_PAGEDOWN:
                    return KeyboardCode.VKEY_NEXT;
                case SDL_Scancode.SDL_SCANCODE_EQUALS:
                    return KeyboardCode.VKEY_OEM_PLUS;
                case SDL_Scancode.SDL_SCANCODE_MINUS:
                    return KeyboardCode.VKEY_OEM_MINUS;
                default:
                    return KeyboardCode.VKEY_UNKNOWN;
            }
        }




        //处理鼠标移动的消息
        private static void HandleMouseMotionEvent(SDL_MouseMotionEvent mouseMotionEvent)
        {
            CefMouseEvent mouseEvent=new CefMouseEvent();
            mouseEvent.X = mouseMotionEvent.x;
            mouseEvent.Y = mouseMotionEvent.y;
            _host.SendMouseMoveEvent(mouseEvent, false);
            //这里暂不处理器modifier
            //mouseEvent.Modifiers=mouseMotionEvent.state.
            //return mouseEvent;
        }

        private static CefMouseButtonType GetMouseButtonFromSDL(SDL_MouseButtonEvent e)
        {
            CefMouseButtonType button;
            switch (e.button)
            {
                case SDL_MouseButton.Left:
                case SDL_MouseButton.X1:
                    button = CefMouseButtonType.Left;
                    break;

                case SDL_MouseButton.Middle:
                    button = CefMouseButtonType.Middle;
                    break;

                case SDL_MouseButton.Right:
                case SDL_MouseButton.X2:
                    button = CefMouseButtonType.Right;
                    break;
                default:
                    button = CefMouseButtonType.Left;
                    break;
            }
            return button;
        }

        //处理鼠标点击事件,up和down一起处理
        private static void HandleMouseButtonEvent(SDL_MouseButtonEvent e)
        {
            CefMouseEvent mouseEvent = new CefMouseEvent() ;
            mouseEvent.X = e.x;
            mouseEvent.Y = e.y;
            _host.SendMouseClickEvent(mouseEvent, GetMouseButtonFromSDL(e), e.state != 1, e.clicks);
        }

        //get modifers of sdl event
       static CefEventFlags GetModifiersFromSDL(SDL_Keymod mod)
        {
            CefEventFlags result = CefEventFlags.None;          
            if ((mod & SDL_Keymod.Num)!=0)
            {
                result |= CefEventFlags.NumLockOn;
            }
            if ((mod & SDL_Keymod.Caps)!=0)
            {
                result |= CefEventFlags.CapsLockOn;
            }
            if ((mod & SDL_Keymod.LeftControl&SDL_Keymod.RightControl)!=0)
            {
                result |= CefEventFlags.ControlDown;
            }
            if ((mod & SDL_Keymod.LeftShift&SDL_Keymod.RightShift)!=0)
            {
                result |= CefEventFlags.ShiftDown;
            }
            if ((mod & SDL_Keymod.LeftAlt&SDL_Keymod.RightAlt)!=0)
            {
                result |= CefEventFlags.AltDown ;
            }
            return result;
        }


    }
}
