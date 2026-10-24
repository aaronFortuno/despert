// Despert - StartupRegistration.cs
// Registre d'inici amb Windows (HKCU\...\Run, nom propi). C# 5, .NET Framework 4.8. Llicència MIT.
using System;
using System.IO;
using Microsoft.Win32;

namespace Despert
{
    public static class StartupRegistration
    {
        private const string RunSubKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "Despert";
        private const int MaxCommandLength = 260;

        // Configuració, no garantia que Windows executi l'aplicació.
        // Els errors es propaguen al caller UI.
        public static bool IsEnabled
        {
            get
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunSubKey, false))
                {
                    if (key == null) return false;
                    object value = key.GetValue(ValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                    string text = value as string;
                    return !string.IsNullOrEmpty(text);
                }
            }
        }

        public static void SetEnabled(bool enabled, string executablePath)
        {
            if (enabled)
            {
                string command = BuildCommand(executablePath);
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunSubKey))
                {
                    if (key == null) throw new IOException(Strings.Get("StartupOpenError"));
                    key.SetValue(ValueName, command, RegistryValueKind.String);
                }
                return;
            }

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunSubKey, true))
            {
                if (key != null) key.DeleteValue(ValueName, false);
            }
        }

        // Pura i testejable: valida i construeix l'ordre d'inici. Cap E/S.
        public static string BuildCommand(string executablePath)
        {
            if (string.IsNullOrEmpty(executablePath))
                throw new ArgumentException(Strings.Get("PathEmpty"), "executablePath");
            string path = executablePath;
            if (path.IndexOf('"') >= 0 || path.IndexOf('\r') >= 0 || path.IndexOf('\n') >= 0 || path.IndexOf('\0') >= 0)
                throw new ArgumentException(Strings.Get("PathInvalid"), "executablePath");
            if (path.Length > MaxCommandLength - 2)
                throw new ArgumentException(Strings.Get("PathLong"), "executablePath");
            string root = Path.GetPathRoot(path);
            bool driveRoot = root.Length == 3 && char.IsLetter(root[0]) && root[1] == ':';
            string[] uncParts = root.Trim('\\').Split('\\');
            bool uncRoot = root.StartsWith(@"\\", StringComparison.Ordinal) && uncParts.Length == 2 &&
                uncParts[0].Length > 0 && uncParts[1].Length > 0 && uncParts[0] != "?" && uncParts[0] != ".";
            if (!driveRoot && !uncRoot)
                throw new ArgumentException(Strings.Get("PathAbsolute"), "executablePath");
            path = Path.GetFullPath(path);
            string extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension) ||
                !string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(Strings.Get("PathExe"), "executablePath");

            string command = "\"" + path + "\"";
            if (command.Length > MaxCommandLength)
                throw new ArgumentException(Strings.Get("PathLong"), "executablePath");
            return command;
        }
    }
}
