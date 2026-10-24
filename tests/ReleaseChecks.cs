// Offline checks: no input injection, power request, or real lock/sleep.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

internal static class CheckRelease
{
    [DllImport("user32.dll")]
    private static extern int GetMenuItemCount(IntPtr menu);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetMenuString(IntPtr menu, uint item, StringBuilder text, int maximum, uint flags);
    [DllImport("user32.dll")]
    private static extern uint GetMenuState(IntPtr menu, uint item, uint flags);
    [DllImport("user32.dll")]
    private static extern uint GetMenuItemID(IntPtr menu, int position);
    private static int count;
    private static void Check(bool result, string label)
    {
        if (!result) throw new InvalidOperationException(label);
        count++;
        Console.WriteLine("PASS " + label);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        try { return Run(args); }
        catch (Exception ex) { Console.Error.WriteLine("FAIL " + ex); return 1; }
    }

    private static int Run(string[] args)
    {
        // The live menu follows CurrentUICulture. Exercise a known language
        // here; LanguageChecks covers all supported regional cultures.
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        Assembly assembly = Assembly.LoadFrom(Path.GetFullPath(args[0]));
        MethodInfo load = assembly.GetType("Despert.DespertContext").GetMethod("LoadIcon", BindingFlags.NonPublic | BindingFlags.Static);
        using (Icon inactive = (Icon)load.Invoke(null, new object[] { "Despert.Inactive.Light.ico" }))
        using (Icon active = (Icon)load.Invoke(null, new object[] { "Despert.Active.Light.ico" }))
        using (Bitmap left = inactive.ToBitmap())
        using (Bitmap right = active.ToBitmap())
        {
            Check(left.Size == right.Size, "both embedded tray icons load after stream disposal");
            int differences = 0;
            for (int y = 0; y < left.Height; y++)
                for (int x = 0; x < left.Width; x++)
                    if (left.GetPixel(x, y) != right.GetPixel(x, y)) differences++;
            Check(differences > 10, "tray states differ at native small-icon size");
        }
        foreach (string state in new string[] { "Inactive.Light", "Active.Light", "Inactive.Dark", "Active.Dark" })
            foreach (int size in new int[] { 12, 16, 20, 24, 32, 40, 48, 64, 128 })
                using (Stream stream = assembly.GetManifestResourceStream("Despert." + state + ".ico"))
                using (Icon icon = new Icon(stream, size, size))
                    Check(icon.Width == size && icon.Height == size, "ICO frame " + state + " " + size);
        // Framework Icon selection treats width=0 (256) differently. Validate
        // the actual encoded frame used by Explorer separately from that selector.
        foreach (string state in new string[] { "Inactive.Light", "Active.Light", "Inactive.Dark", "Active.Dark" })
            using (Stream stream = assembly.GetManifestResourceStream("Despert." + state + ".ico"))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.ReadUInt16(); reader.ReadUInt16();
                int frames = reader.ReadUInt16();
                Check(frames == 10, "ICO directory has all ten sizes " + state);
                stream.Position = 6 + 16 * (frames - 1);
                Check(reader.ReadByte() == 0 && reader.ReadByte() == 0, "ICO 256 directory entry " + state);
                reader.ReadBytes(6);
                int length = reader.ReadInt32();
                int offset = reader.ReadInt32();
                stream.Position = offset;
                using (MemoryStream encoded = new MemoryStream(reader.ReadBytes(length)))
                using (Bitmap frame = new Bitmap(encoded))
                    Check(frame.Width == 256 && frame.Height == 256, "ICO 256 PNG decodes " + state);
            }
        foreach (string theme in new string[] { "Light", "Dark" })
            foreach (int size in new int[] { 12, 16 })
                using (Stream offStream = assembly.GetManifestResourceStream("Despert.Inactive." + theme + ".ico"))
                using (Stream onStream = assembly.GetManifestResourceStream("Despert.Active." + theme + ".ico"))
                using (Icon offIcon = new Icon(offStream, size, size))
                using (Icon onIcon = new Icon(onStream, size, size))
                using (Bitmap off = offIcon.ToBitmap())
                using (Bitmap on = onIcon.ToBitmap())
                {
                    int offSteam = 0, onSteam = 0;
                    bool monochrome = true;
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                        {
                            Color pixel = on.GetPixel(x, y);
                            if (pixel.A > 0 && (pixel.R != pixel.G || pixel.G != pixel.B)) monochrome = false;
                            if (y < size * 0.4)
                            {
                                if (off.GetPixel(x, y).A > 64) offSteam++;
                                if (pixel.A > 64) onSteam++;
                            }
                        }
                    Check(monochrome, "monochrome tray " + theme + " " + size);
                    Check(offSteam == 0 && onSteam >= 4, "steam distinguishes active at " + size + " px " + theme);
                }
        CheckNativeMenu(assembly);
        using (Icon explorer = Icon.ExtractAssociatedIcon(Path.GetFullPath(args[0])))
        using (Bitmap bitmap = explorer.ToBitmap())
        {
            bitmap.Save(Path.Combine(args[1], "explorer-icon.png"));
            Check(bitmap.Width > 0, "Explorer executable icon extracts");
        }
        Type receiverType = assembly.GetType("Despert.SessionEvents");
        object receiver = Activator.CreateInstance(receiverType);
        try
        {
            PropertyInfo paused = receiverType.GetProperty("Paused");
            MethodInfo windowMessage = receiverType.GetMethod("WndProc", BindingFlags.NonPublic | BindingFlags.Instance);
            Action<int, int> send = delegate(int message, int code)
            {
                // Direct method invocation on the test receiver; not a system broadcast.
                Message m = Message.Create(IntPtr.Zero, message, new IntPtr(code), IntPtr.Zero);
                windowMessage.Invoke(receiver, new object[] { m });
            };
            Check(!(bool)paused.GetValue(receiver, null), "initial session unpaused");
            send(0x02B1, 7);
            Check((bool)paused.GetValue(receiver, null), "lock pauses");
            send(0x0218, 4);
            send(0x02B1, 8);
            Check((bool)paused.GetValue(receiver, null), "unlock does not override suspension");
            send(0x0218, 18);
            Check(!(bool)paused.GetValue(receiver, null), "automatic resume decimal 18 clears suspension");
            send(0x0218, 7);
            Check(!(bool)paused.GetValue(receiver, null), "duplicate resume stays unpaused");
            send(0x02B1, 4); send(0x02B1, 7); send(0x02B1, 3);
            Check((bool)paused.GetValue(receiver, null), "reconnect does not override lock");
            send(0x02B1, 8);
            Check(!(bool)paused.GetValue(receiver, null), "unlock after reconnect unpauses");
        }
        finally { ((IDisposable)receiver).Dispose(); }
        Console.WriteLine("Release checks passed: " + count);
        return 0;
    }

    private static void CheckNativeMenu(Assembly assembly)
    {
        Type type = assembly.GetType("Despert.NativeMenu");
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        MethodInfo create = type.GetMethod("CreatePopupMenu", flags);
        MethodInfo insert = type.GetMethod("InsertItem", flags);
        MethodInfo destroy = type.GetMethod("DestroyMenu", flags);
        MethodInfo populate = type.GetMethod("PopulateMenu", flags);
        foreach (bool active in new bool[] { false, true })
            foreach (bool startup in new bool[] { false, true })
                foreach (bool available in new bool[] { false, true })
                {
                    IntPtr configured = (IntPtr)create.Invoke(null, null);
                    List<IntPtr> texts = new List<IntPtr>(4);
                    try
                    {
                        populate.Invoke(null, new object[] { configured, active, startup, available, texts });
                        Check(GetMenuItemCount(configured) == 6, "complete menu has four actions and two separators");
                        Check(GetMenuItemID(configured, 0) == 1 && GetMenuItemID(configured, 2) == 3 &&
                            GetMenuItemID(configured, 3) == 4 && GetMenuItemID(configured, 5) == 2,
                            "complete menu command order keeps exit last");
                        Check((GetMenuState(configured, 1, 0x400) & 0x800) != 0 &&
                            (GetMenuState(configured, 4, 0x400) & 0x800) != 0, "native separators have separator type");
                        uint state = GetMenuState(configured, 2, 0x400);
                        Check(((state & 8) != 0) == startup, "startup check follows configured state");
                        Check(((state & 3) != 0) == !available, "startup unavailable disables only its item");
                        Check((GetMenuState(configured, 0, 0x400) & 3) == 0 &&
                            (GetMenuState(configured, 3, 0x400) & 3) == 0 &&
                            (GetMenuState(configured, 5, 0x400) & 3) == 0, "other actions remain available");
                        StringBuilder text = new StringBuilder(128);
                        GetMenuString(configured, 0, text, text.Capacity, 0x400);
                        Check(text.ToString() == (active ? "&Deactivate" : "&Activate"), "live toggle text follows active state");
                        text.Clear();
                        GetMenuString(configured, 3, text, text.Capacity, 0x400);
                        Check(text.ToString() == "A&bout…", "live About action retains Unicode");
                    }
                    finally
                    {
                        destroy.Invoke(null, new object[] { configured });
                        foreach (IntPtr text in texts) Marshal.FreeCoTaskMem(text);
                    }
                }
        foreach (string toggle in new string[] { "&Activar", "&Desactivar" })
        {
            IntPtr menu = (IntPtr)create.Invoke(null, null);
            IntPtr first = IntPtr.Zero, second = IntPtr.Zero;
            try
            {
                first = (IntPtr)insert.Invoke(null, new object[] { menu, (uint)1, toggle });
                second = (IntPtr)insert.Invoke(null, new object[] { menu, (uint)2, "&Surt" });
                Check(GetMenuItemCount(menu) == 2, "native menu has exactly two items");
                StringBuilder text = new StringBuilder(64);
                GetMenuString(menu, 0, text, text.Capacity, 0x400);
                Check(text.ToString() == toggle, "native Unicode toggle text " + toggle);
                text.Clear();
                GetMenuString(menu, 1, text, text.Capacity, 0x400);
                Check(text.ToString() == "&Surt", "native Unicode exit text");
            }
            finally
            {
                destroy.Invoke(null, new object[] { menu });
                if (first != IntPtr.Zero) Marshal.FreeCoTaskMem(first);
                if (second != IntPtr.Zero) Marshal.FreeCoTaskMem(second);
            }
        }
        MethodInfo theme = type.GetMethod("LoadUxThemeDarkApis", flags);
        IntPtr module = (IntPtr)theme.Invoke(null, new object[] { IntPtr.Zero });
        Console.WriteLine("Native dark-menu integration available: " + (module != IntPtr.Zero));
        if (module != IntPtr.Zero) type.GetMethod("FreeLibrary", flags).Invoke(null, new object[] { module });
    }
}
