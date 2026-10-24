// Comprova el contracte binari amb Win32. No crida cap API nativa.
using System;
using System.Runtime.InteropServices;
using System.Reflection;
using Despert;

namespace DespertTests
{
    internal static class InteropTests
    {
        public static int Main()
        {
            int expectedSize = IntPtr.Size == 8 ? 40 : 28;
            int expectedUnionOffset = IntPtr.Size == 8 ? 8 : 4;
            int expectedExtraOffset = IntPtr.Size == 8 ? 24 : 20;
            bool valid = Native.InputSize == expectedSize &&
                Marshal.OffsetOf(typeof(Native.INPUT), "union").ToInt32() == expectedUnionOffset &&
                Marshal.OffsetOf(typeof(Native.MOUSEINPUT), "dwExtraInfo").ToInt32() == expectedExtraOffset;
            Console.WriteLine((valid ? "PASS" : "FAIL") + " Win32 INPUT " + (IntPtr.Size * 8) +
                " bits: mida=" + Native.InputSize + ", unio=" +
                Marshal.OffsetOf(typeof(Native.INPUT), "union") + ", punter=" +
                Marshal.OffsetOf(typeof(Native.MOUSEINPUT), "dwExtraInfo"));
            Type menuInfo = typeof(NativeMenu).GetNestedType("MENUITEMINFO", BindingFlags.NonPublic);
            bool menuValid = Marshal.SizeOf(menuInfo) == (IntPtr.Size == 8 ? 80 : 48) &&
                Marshal.OffsetOf(menuInfo, "dwItemData").ToInt32() == (IntPtr.Size == 8 ? 48 : 32);
            Console.WriteLine((menuValid ? "PASS" : "FAIL") + " Win32 MENUITEMINFO " + (IntPtr.Size * 8) + " bits");
            return valid && menuValid ? 0 : 1;
        }
    }
}
