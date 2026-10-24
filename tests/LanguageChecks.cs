// Builds dialogs offscreen; never starts Despert, follows a link or uses clipboard.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

internal static class LanguageChecks
{
    private static int count;
    private static void Check(bool result, string message)
    {
        if (!result) throw new InvalidOperationException(message);
        count++;
    }

    private static string Markers(string text)
    {
        List<string> markers = new List<string>();
        foreach (Match marker in Regex.Matches(text, @"\{[0-9]+\}")) markers.Add(marker.Value);
        markers.Sort(StringComparer.Ordinal);
        return string.Join(",", markers.ToArray());
    }

    private static void CreateHandles(Control control)
    {
        IntPtr handle = control.Handle;
        control.PerformLayout();
        foreach (Control child in control.Controls) CreateHandles(child);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        CultureInfo original = CultureInfo.CurrentUICulture;
        try
        {
            Application.EnableVisualStyles();
            Assembly assembly = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            Type strings = assembly.GetType("Despert.Strings");
            MethodInfo language = strings.GetMethod("LanguageFor");
            MethodInfo get = strings.GetMethod("GetFor");
            Dictionary<string, string[]> translations = (Dictionary<string, string[]>)assembly.GetType("Despert.Translations")
                .GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            foreach (KeyValuePair<string, string[]> entry in translations)
            {
                Check(entry.Value.Length == 5, "five translations: " + entry.Key);
                foreach (string value in entry.Value)
                {
                    Check(!string.IsNullOrWhiteSpace(value), "nonempty translation: " + entry.Key);
                    Check(Markers(value) == Markers(entry.Value[0]), "matching placeholders: " + entry.Key);
                    string.Format(CultureInfo.InvariantCulture, value, "0", "1");
                }
            }
            string[] cultures = { "en-US", "ca-ES", "es-ES", "eu-ES", "gl-ES", "es-MX", "fr-FR", "zh-CN" };
            string[] expected = { "en", "ca", "es", "eu", "gl", "es", "en", "en" };
            for (int index = 0; index < cultures.Length; index++)
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(cultures[index]);
                Check((string)language.Invoke(null, new object[] { culture }) == expected[index], "regional language: " + cultures[index]);
                if (index >= 6)
                    Check((string)get.Invoke(null, new object[] { culture, "Close" }) == "Close", "unsupported language falls back to English");
            }
            Check((string)language.Invoke(null, new object[] { null }) == "en", "null culture fallback");
            Check((string)language.Invoke(null, new object[] { CultureInfo.InvariantCulture }) == "en", "invariant culture fallback");
            CheckQr(assembly, args[1]);
            foreach (string cultureName in new string[] { "en-US", "ca-ES", "es-ES", "eu-ES", "gl-ES" })
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
                foreach (string state in new string[] { "Activate", "Deactivate" })
                {
                    HashSet<char> shortcuts = new HashSet<char>();
                    foreach (string key in new string[] { state, "StartWindows", "About", "Exit" })
                    {
                        string text = (string)get.Invoke(null, new object[] { CultureInfo.CurrentUICulture, key });
                        int ampersand = text.IndexOf('&');
                        Check(ampersand >= 0 && ampersand < text.Length - 1, "menu keyboard shortcut: " + key);
                        Check(shortcuts.Add(char.ToUpperInvariant(text[ampersand + 1])), "unique menu shortcut: " + cultureName + "/" + key);
                    }
                }
                using (Form dialog = (Form)Activator.CreateInstance(assembly.GetType("Despert.AboutDialog")))
                {
                    CreateHandles(dialog);
                    dialog.PerformLayout();
                    Check(dialog.AcceptButton != null && dialog.CancelButton != null, "dialog supports Enter and Escape");
                    Check(dialog.Width < 700 && dialog.Height < 850, "compact About dialog: " + cultureName);
                    using (Bitmap bitmap = new Bitmap(dialog.Width, dialog.Height))
                    {
                        dialog.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                        bitmap.Save(Path.Combine(args[1], "about-" + cultureName + ".png"));
                    }
                }
                Console.WriteLine("PASS language and offscreen About dialog " + cultureName);
            }
            Console.WriteLine("Language checks passed: " + count);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("FAIL " + ex); return 1; }
        finally { Thread.CurrentThread.CurrentUICulture = original; }
    }

    private static void CheckQr(Assembly assembly, string output)
    {
        List<string> rows = new List<string>();
        using (Stream stream = assembly.GetManifestResourceStream("Despert.Lightning.modules"))
        using (StreamReader reader = new StreamReader(stream))
        {
            string row;
            while ((row = reader.ReadLine()) != null) rows.Add(row);
        }
        Check(rows.Count == 45, "QR module count includes quiet zone");
        using (Control qr = (Control)Activator.CreateInstance(assembly.GetType("Despert.LightningQr")))
            foreach (int size in new int[] { 135, 180, 225, 270 })
            {
                qr.Size = new Size(size, size);
                CreateHandles(qr);
                using (Bitmap bitmap = new Bitmap(size, size))
                {
                    qr.DrawToBitmap(bitmap, new Rectangle(0, 0, size, size));
                    int module = size / 45;
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                        {
                            bool quiet = x / module < 4 || y / module < 4 || x / module >= 41 || y / module >= 41;
                            bool black = rows[y / module][x / module] == '1';
                            if (quiet && black) throw new InvalidOperationException("QR quiet zone must stay white");
                            if (bitmap.GetPixel(x, y).ToArgb() != (black ? Color.Black : Color.White).ToArgb())
                                throw new InvalidOperationException("QR module rendering changed at " + size);
                        }
                    Check(true, "QR modules are exact black/white pixels at " + size);
                    bitmap.Save(Path.Combine(output, "lightning-" + size + ".png"));
                }
            }
    }
}
