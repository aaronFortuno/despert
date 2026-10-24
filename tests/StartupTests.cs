// Pure validation only: never reads/writes Run, never starts the app or Settings.
using System;
using System.IO;
using System.Reflection;

internal static class StartupTests
{
    private static int count;
    private static MethodInfo build;

    private static void Check(bool result, string label)
    {
        if (!result) throw new InvalidOperationException(label);
        count++;
        Console.WriteLine("PASS " + label);
    }

    private static string Command(string path)
    {
        return (string)build.Invoke(null, new object[] { path });
    }

    private static void Reject(string path, string label)
    {
        try { Command(path); }
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException is ArgumentException)
            {
                Check(true, label);
                return;
            }
            throw;
        }
        Check(false, label);
    }

    private static int Main(string[] args)
    {
        try
        {
            Assembly assembly = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            build = assembly.GetType("Despert.StartupRegistration").GetMethod("BuildCommand");
            string path = @"C:\Equip docent\Àrea personal\Despert.exe";
            Check(Command(path) == "\"" + path + "\"", "spaces and Unicode use one quoted executable path");
            path = @"\\servidor\carpeta compartida\Despert.exe";
            Check(Command(path) == "\"" + path + "\"", "absolute UNC path stays quoted");
            path = @"C:\literal %TEMP%\Despert.exe";
            Check(Command(path) == "\"" + path + "\"", "no environment expansion in REG_SZ command");
            Reject(null, "null path rejected");
            Reject("", "empty path rejected");
            Reject("Despert.exe", "relative path rejected");
            Reject(@"C:Despert.exe", "drive-relative path rejected");
            Reject(@"\Despert.exe", "root-relative path rejected");
            Reject("\"C:\\Despert.exe\"", "prequoted path rejected");
            Reject("C:\\Despert.exe\" --other", "argument injection rejected");
            Reject("C:\\line\r\nbreak.exe", "line break rejected");
            Reject("C:\\nul\0.exe", "NUL rejected");
            path = @"C:\" + new string('a', 251) + ".exe";
            Check(Command(path).Length == 260, "documented Run length boundary accepted");
            Reject(@"C:\" + new string('a', 252) + ".exe", "overlong Run command rejected");
            Console.WriteLine("Startup checks passed: " + count);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("FAIL " + ex); return 1; }
    }
}
