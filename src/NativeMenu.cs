// Despert - NativeMenu.cs
// Menú contextual nadiu Win32 i aparença. C# 5, .NET Framework 4.8. Llicència MIT.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Despert
{
    public static class Appearance
    {
        private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        // Registre només lectura; mai Create/Write. En error previsible, llum per fallback de ControlText.
        public static bool TaskbarDark
        {
            get
            {
                if (SystemInformation.HighContrast) return !IsDark(SystemColors.MenuText);
                return ReadDark("SystemUsesLightTheme");
            }
        }

        // En alt contrast es respecta MenuText i no es força mai el tema fosc del menú.
        public static bool AppsDark
        {
            get
            {
                if (SystemInformation.HighContrast) return false;
                return ReadDark("AppsUseLightTheme");
            }
        }

        private static bool ReadDark(string valueName)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(PersonalizeKey, false))
                {
                    if (key == null) return FallbackDark();
                    object value = key.GetValue(valueName);
                    if (value is int) return ((int)value) == 0; // 0 = tema fosc
                    return FallbackDark();
                }
            }
            catch (SecurityException) { return FallbackDark(); }
            catch (UnauthorizedAccessException) { return FallbackDark(); }
            catch (IOException) { return FallbackDark(); }
            catch (ArgumentException) { return FallbackDark(); }
            catch (InvalidCastException) { return FallbackDark(); }
            catch (OverflowException) { return FallbackDark(); }
            catch (FormatException) { return FallbackDark(); }
        }

        private static bool FallbackDark()
        {
            // Light foreground text implies a dark background, not vice versa.
            return !IsDark(SystemColors.ControlText);
        }

        private static bool IsDark(Color c)
        {
            // Llindar perceptual senzill per a tinta de menú sobre fons clar/fosc.
            return ((int)c.R + (int)c.G + (int)c.B) < 384;
        }
    }

    public static class NativeMenu
    {
        private const uint TPM_RETURNCMD = 0x0100;
        private const uint TPM_NONOTIFY = 0x0080;
        private const uint TPM_RIGHTBUTTON = 0x0002;
        private const uint MIIM_ID = 0x0002;
        private const uint MIIM_STRING = 0x0040;
        private const uint MIIM_STATE = 0x0001;
        private const uint MIIM_FTYPE = 0x0100;
        private const uint MFS_CHECKED = 0x0008;
        private const uint MFS_DISABLED = 0x0003;
        private const uint MFT_SEPARATOR = 0x0800;
        private const int WM_NULL = 0x0000;
        private const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;
        private const int PreferredAppModeDefault = 0;
        private const int PreferredAppModeForceDark2 = 2;   // ordinal 135, no documentat
        private const int PreferredAppModeForceLight3 = 3;  // ordinal 135, no documentat

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MENUITEMINFO
        {
            public uint cbSize;
            public uint fMask;
            public uint fType;
            public uint fState;
            public uint wID;
            public IntPtr hSubMenu;
            public IntPtr hbmpChecked;
            public IntPtr hbmpUnchecked;
            public UIntPtr dwItemData;
            public IntPtr dwTypeData;
            public uint cch;
            public IntPtr hbmpItem;
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate int SetPreferredAppModeDelegate(int mode); // uxtheme ordinal 135, no documentat
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool AllowDarkModeForWindowDelegate(IntPtr hwnd, [MarshalAs(UnmanagedType.I1)] bool allow); // ordinal 133
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate void FlushMenuThemesDelegate(); // uxtheme ordinal 136, no documentat

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InsertMenuItemW(IntPtr hMenu, uint uItem, [MarshalAs(UnmanagedType.Bool)] bool fByPosition, ref MENUITEMINFO lpmii);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int TrackPopupMenuEx(IntPtr hMenu, uint uFlags, int x, int y, IntPtr hWnd, IntPtr lptpm);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReserved, uint dwFlags);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, IntPtr ordinal);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        public static int Show(IntPtr owner, bool active, bool startupEnabled, bool startupAvailable)
        {
            IntPtr menu = IntPtr.Zero;
            IntPtr uxtheme = IntPtr.Zero;
            List<IntPtr> texts = new List<IntPtr>(4);
            try
            {
                // Optatiu i protegit: uxtheme.dll només des de System32 i sense ordinals estàtics.
                uxtheme = LoadUxThemeDarkApis(owner);

                menu = CreatePopupMenu();
                if (menu == IntPtr.Zero) throw new Win32Exception();

                PopulateMenu(menu, active, startupEnabled, startupAvailable, texts);

                POINT pt;
                if (!GetCursorPos(out pt)) throw new Win32Exception();

                // Windows pot denegar el focus; no és un error del menú.
                SetForegroundWindow(owner);

                return TrackPopupMenuEx(menu, TPM_RETURNCMD | TPM_NONOTIFY | TPM_RIGHTBUTTON,
                    pt.X, pt.Y, owner, IntPtr.Zero); // 0 = cancel·lat, és normal
            }
            finally
            {
                PostMessage(owner, WM_NULL, IntPtr.Zero, IntPtr.Zero);
                if (menu != IntPtr.Zero) DestroyMenu(menu);
                foreach (IntPtr text in texts) Marshal.FreeCoTaskMem(text);
                if (uxtheme != IntPtr.Zero) FreeLibrary(uxtheme);
            }
        }

        // Shared by the live menu and offline checks. Caller owns menu and texts.
        private static void PopulateMenu(IntPtr menu, bool active, bool startupEnabled, bool startupAvailable, List<IntPtr> texts)
        {
            texts.Add(InsertItem(menu, 1, Strings.Get(active ? "Deactivate" : "Activate")));
            InsertSeparator(menu);
            texts.Add(InsertStartupItem(menu, 3, Strings.Get("StartWindows") + (startupAvailable ? "" : " (" + Strings.Get("Unavailable") + ")"),
                startupEnabled, startupAvailable));
            texts.Add(InsertItem(menu, 4, Strings.Get("About")));
            InsertSeparator(menu);
            texts.Add(InsertItem(menu, 2, Strings.Get("Exit")));
        }

        private static IntPtr InsertItem(IntPtr menu, uint id, string text)
        {
            MENUITEMINFO item = new MENUITEMINFO();
            item.cbSize = (uint)Marshal.SizeOf(typeof(MENUITEMINFO));
            item.fMask = MIIM_ID | MIIM_STRING;
            item.fType = 0;
            item.fState = 0; // habilitat
            item.wID = id;
            item.dwItemData = UIntPtr.Zero;
            item.dwTypeData = Marshal.StringToCoTaskMemUni(text);
            item.cch = (uint)text.Length;
            // Sempre s'afegeix al final: permet insercions fora d'ordre d'ID.
            if (!InsertMenuItemW(menu, uint.MaxValue, true, ref item))
            {
                IntPtr leaked = item.dwTypeData;
                if (leaked != IntPtr.Zero) Marshal.FreeCoTaskMem(leaked);
                throw new Win32Exception();
            }
            return item.dwTypeData;
        }

        private static void InsertSeparator(IntPtr menu)
        {
            MENUITEMINFO item = new MENUITEMINFO();
            item.cbSize = (uint)Marshal.SizeOf(typeof(MENUITEMINFO));
            item.fMask = MIIM_FTYPE;
            item.fType = MFT_SEPARATOR;
            if (!InsertMenuItemW(menu, uint.MaxValue, true, ref item))
                throw new Win32Exception();
        }

        private static IntPtr InsertStartupItem(IntPtr menu, uint id, string text, bool enabled, bool available)
        {
            MENUITEMINFO item = new MENUITEMINFO();
            item.cbSize = (uint)Marshal.SizeOf(typeof(MENUITEMINFO));
            item.fMask = MIIM_ID | MIIM_STRING | MIIM_STATE;
            item.fType = 0;
            item.fState = enabled ? MFS_CHECKED : 0;
            if (!available) item.fState |= MFS_DISABLED;
            item.wID = id;
            item.dwItemData = UIntPtr.Zero;
            item.dwTypeData = Marshal.StringToCoTaskMemUni(text);
            item.cch = (uint)text.Length;
            if (!InsertMenuItemW(menu, uint.MaxValue, true, ref item))
            {
                IntPtr leaked = item.dwTypeData;
                if (leaked != IntPtr.Zero) Marshal.FreeCoTaskMem(leaked);
                throw new Win32Exception();
            }
            return item.dwTypeData;
        }

        private static IntPtr LoadUxThemeDarkApis(IntPtr owner)
        {
            // Les ordinals 133/135/136 són interns i no documentats. Són optatives:
            // si falten, o el SO no és compatible, el menú nadiu estàndard segueix usable.
            if (Environment.OSVersion.Version < new Version(10, 0, 18362)) return IntPtr.Zero;

            string path = Path.Combine(Environment.SystemDirectory, "uxtheme.dll");
            IntPtr ux = LoadLibraryEx(path, IntPtr.Zero, LOAD_LIBRARY_SEARCH_SYSTEM32);
            if (ux == IntPtr.Zero) return IntPtr.Zero;

            IntPtr pSet = GetProcAddress(ux, new IntPtr(135));
            IntPtr pAllow = GetProcAddress(ux, new IntPtr(133));
            IntPtr pFlush = GetProcAddress(ux, new IntPtr(136));
            if (pSet == IntPtr.Zero || pAllow == IntPtr.Zero || pFlush == IntPtr.Zero)
            {
                FreeLibrary(ux);
                return IntPtr.Zero;
            }

            int mode = PreferredAppModeDefault;
            bool allowDark = false;
            try
            {
                bool highContrast = SystemInformation.HighContrast;
                bool appsDark = Appearance.AppsDark;
                mode = highContrast ? PreferredAppModeDefault : (appsDark ? PreferredAppModeForceDark2 : PreferredAppModeForceLight3);
                allowDark = !highContrast && appsDark; // en alt contrast no es força fosc
            }
            catch
            {
                mode = PreferredAppModeDefault;
                allowDark = false;
            }

            try
            {
                SetPreferredAppModeDelegate setPreferred = (SetPreferredAppModeDelegate)Marshal.GetDelegateForFunctionPointer(pSet, typeof(SetPreferredAppModeDelegate));
                AllowDarkModeForWindowDelegate allow = (AllowDarkModeForWindowDelegate)Marshal.GetDelegateForFunctionPointer(pAllow, typeof(AllowDarkModeForWindowDelegate));
                FlushMenuThemesDelegate flush = (FlushMenuThemesDelegate)Marshal.GetDelegateForFunctionPointer(pFlush, typeof(FlushMenuThemesDelegate));

                setPreferred(mode);
                allow(owner, allowDark);
                flush();
            }
            catch
            {
                // Les API fosc són opcionals; no han d'impedir el menú estàndard.
            }
            return ux;
        }
    }
}
