// Despert - Native.cs
// P/Invoke Win32 limitat: entrada d'usuari, estat d'energia i impulsos de ratolí.
// Cap xarxa, hooks, captura de teclat, registre, polítiques ni persistència.
// Llicència MIT.
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Despert
{
    public static class Native
    {
        public static int InputSize { get { return Marshal.SizeOf(typeof(INPUT)); } }

        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;

        private const uint MOUSEEVENTF_MOVE = 0x0001;

        private const int VK_LBUTTON = 0x01;
        private const int VK_RBUTTON = 0x02;
        private const int VK_MBUTTON = 0x04;
        private const int VK_XBUTTON1 = 0x05;
        private const int VK_XBUTTON2 = 0x06;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs,
            [MarshalAs(UnmanagedType.LPArray)] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public INPUTUNION union;
        }

        // Unió amb mida total correcta: INPUT = 40 bytes x64, 28 bytes x86.
        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        // Unió ABI completa; no s'utilitza per generar tecles o altres entrades.
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk, wScan;
            public uint dwFlags, time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL, wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo; // UIntPtr punter
        }

        // Millisegons d'inactivitat des de l'última entrada (GetLastInputInfo).
        public static uint IdleMs()
        {
            LASTINPUTINFO lii = new LASTINPUTINFO();
            lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
            if (!GetLastInputInfo(ref lii))
                throw new InvalidOperationException(Strings.Get("IdleError"));
            return Session.IdleMilliseconds(GetTickCount(), lii.dwTime);
        }

        // Cert si algun dels 5 botons del ratolí està premut. Només ratolí.
        public static bool ButtonsHeld()
        {
            return (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0 ||
                   (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0 ||
                   (GetAsyncKeyState(VK_MBUTTON) & 0x8000) != 0 ||
                   (GetAsyncKeyState(VK_XBUTTON1) & 0x8000) != 0 ||
                   (GetAsyncKeyState(VK_XBUTTON2) & 0x8000) != 0;
        }

        public static void HoldPower()
        {
            uint r = SetThreadExecutionState(
                ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
            if (r == 0)
                throw new InvalidOperationException(
                    Strings.Get("PowerError"));
            // Nota: un retorn correcte no garanteix efectivitat contra polítiques.
        }

        public static void ReleasePower()
        {
            SetThreadExecutionState(ES_CONTINUOUS);
            // Alliberament en el millor esforç; cap bucle d'errors.
        }

        // Injecta un impuls de ratolí segons el mode.
        // Nota: la unitat relativa MOUSEEVENTF_MOVE no garanteix un píxel
        // visible exacte ni compensació amb totes les configuracions de DPI.
        public static void Pulse(KeepMode mode)
        {
            if (mode == KeepMode.ZeroMove)
            {
                INPUT[] inputs = new INPUT[1];
                inputs[0].type = 0; // INPUT_MOUSE
                inputs[0].union.mi = MakeMove(0, 0);
                SendChecked(inputs);
            }
            else if (mode == KeepMode.MinimalMove)
            {
                // Direcció inicial cap a l'interior des de la vora inferior.
                // Sense restaurar cap posició absoluta antiga.
                POINT pt;
                if (!GetCursorPos(out pt))
                    throw new InvalidOperationException(Strings.Get("PointerError"));
                Rectangle bounds = Screen.FromPoint(new Point(pt.X, pt.Y)).Bounds;
                int dy = pt.Y >= bounds.Bottom - 8 ? -1 : 1;

                INPUT[] inputs = new INPUT[2];
                inputs[0].type = 0;
                inputs[0].union.mi = MakeMove(0, dy);
                inputs[1].type = 0;
                inputs[1].union.mi = MakeMove(0, -dy);
                SendChecked(inputs);
            }
            // PowerOnly: cap injecció.
        }

        private static MOUSEINPUT MakeMove(int dx, int dy)
        {
            MOUSEINPUT mi = new MOUSEINPUT();
            mi.dx = dx;
            mi.dy = dy;
            mi.mouseData = 0;
            mi.dwFlags = MOUSEEVENTF_MOVE;
            mi.time = 0;
            mi.dwExtraInfo = IntPtr.Zero;
            return mi;
        }

        private static void SendChecked(INPUT[] inputs)
        {
            uint sent = SendInput((uint)inputs.Length, inputs, InputSize);
            if (sent != (uint)inputs.Length)
                throw new InvalidOperationException(
                    Strings.Format("InputError", sent, inputs.Length));
            // Sense correcció absoluta ni fallback ocult.
        }
    }
}
