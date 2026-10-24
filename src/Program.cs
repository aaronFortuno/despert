// Despert - Program.cs
// Aplicació de safata (NotifyIcon) en català. C# 5, .NET Framework 4.8.
// Llicència MIT.
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

[assembly: AssemblyTitle("Despert")]
[assembly: AssemblyProduct("Despert")]
[assembly: AssemblyCompany("Aarón Fortuño")]
[assembly: AssemblyDescription("One-click Windows tray utility to keep your computer awake.")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Aarón Fortuño and Despert contributors")]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyFileVersion("1.0.1.0")]

namespace Despert
{
    public sealed class SessionEvents : NativeWindow, IDisposable
    {
        private const int WM_WTSSESSION_CHANGE = 0x02B1;
        private const int WM_POWERBROADCAST = 0x0218;
        private const int WM_SETTINGCHANGE = 0x001A;
        private const int WM_THEMECHANGED = 0x031A;
        private const int WM_DISPLAYCHANGE = 0x007E;
        private const int WM_DPICHANGED = 0x02E0;
        private const int NOTIFY_FOR_THIS_SESSION = 0;

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);

        // Els avisos s'executen al mateix fil que la safata.
        public event Action LockChanged;
        public event Action SuspendChanged;
        public event Action DisconnectedChanged;
        public event Action Logoff;
        public event Action AppearanceChanged;

        private bool _locked;
        private bool _suspended;
        private bool _disconnected;

        public bool Paused { get { return _locked || _suspended || _disconnected; } }

        public SessionEvents()
        {
            CreateParams cp = new CreateParams();
            cp.Caption = "DespertEvents";
            cp.Parent = IntPtr.Zero;
            CreateHandle(cp);
            if (!WTSRegisterSessionNotification(Handle, NOTIFY_FOR_THIS_SESSION))
            {
                DestroyHandle();
                throw new InvalidOperationException(Strings.Get("SessionError"));
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_WTSSESSION_CHANGE)
            {
                switch (m.WParam.ToInt32())
                {
                    case 7: SetFlag(ref _locked, true, LockChanged); break;          // lock
                    case 8: SetFlag(ref _locked, false, LockChanged); break;         // unlock
                    case 4: SetFlag(ref _disconnected, true, DisconnectedChanged); break;  // remota desconnecta
                    case 3: SetFlag(ref _disconnected, false, DisconnectedChanged); break; // remota connecta
                    case 2: SetFlag(ref _disconnected, true, DisconnectedChanged); break;  // consola desconnecta
                    case 1: SetFlag(ref _disconnected, false, DisconnectedChanged); break; // consola connecta
                    case 6: // logoff: alliberar i parar; el SO tancarà el procés
                        if (Logoff != null) Logoff();
                        break;
                }
            }
            else if (m.Msg == WM_POWERBROADCAST)
            {
                int code = m.WParam.ToInt32();
                if (code == 4) SetFlag(ref _suspended, true, SuspendChanged);        // suspend
                else if (code == 7 || code == 18) SetFlag(ref _suspended, false, SuspendChanged); // PBT_APMRESUMEAUTOMATIC = 0x12
            }
            else if (m.Msg == WM_SETTINGCHANGE || m.Msg == WM_THEMECHANGED ||
                     m.Msg == WM_DISPLAYCHANGE || m.Msg == WM_DPICHANGED)
            {
                // Fil UI de NativeWindow: no cal marshaling addicional.
                if (AppearanceChanged != null) AppearanceChanged();
            }
            base.WndProc(ref m);
        }

        // Només notifica si el flag realment canvia; així resume repetits no fan res.
        private void SetFlag(ref bool flag, bool value, Action handler)
        {
            if (flag == value) return;
            flag = value;
            if (handler != null) handler();
        }

        public void Dispose()
        {
            if (Handle == IntPtr.Zero) return;
            WTSUnRegisterSessionNotification(Handle);
            DestroyHandle();
        }
    }

    public class DespertContext : ApplicationContext
    {
        private readonly NotifyIcon _tray;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Session _session;
        private readonly SessionEvents _events;
        private readonly Stopwatch _run;
        private Icon _iconInactive;
        private Icon _iconActive;
        private bool _taskbarDark;
        private Size _iconSize;
        private bool _paused;       // pausa efectiva d'efectes (session.Paused)
        private bool _disposed;
        private string _lastError;
        private bool _errorShown;
        private AboutDialog _aboutDialog;

        public DespertContext()
        {
            _session = new Session();
            _session.SetMode(KeepMode.ZeroMove);
            _session.SetIdleSeconds(45);
            _session.SetAcOnly(false);
            _run = Stopwatch.StartNew();
            try
            {
                _taskbarDark = Appearance.TaskbarDark;
                _iconSize = SystemInformation.SmallIconSize;
                _iconInactive = LoadIcon(_taskbarDark ? "Despert.Inactive.Dark.ico" : "Despert.Inactive.Light.ico");
                _iconActive = LoadIcon(_taskbarDark ? "Despert.Active.Dark.ico" : "Despert.Active.Light.ico");
                _events = new SessionEvents();
                _events.LockChanged += OnPauseChanged;
                _events.SuspendChanged += OnPauseChanged;
                _events.DisconnectedChanged += OnPauseChanged;
                _events.Logoff += delegate { _session.Stop(); ReleaseEffects(); UpdateStatus(); };
                _events.AppearanceChanged += RefreshAppearance;
                _tray = new NotifyIcon();
                _tray.MouseClick += OnTrayClick;
                UpdateStatus();
                _tray.Visible = true;
                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = 1000;
                _timer.Tick += OnTick;
                _timer.Start();
            }
            catch { Dispose(); throw; }
        }

        private static Icon LoadIcon(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream stream = asm.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException(Strings.Format("MissingIcon", resourceName));
                using (Icon temp = new Icon(stream, SystemInformation.SmallIconSize))
                    return (Icon)temp.Clone();
            }
        }

        private void OnTrayClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ToggleActive();
                return;
            }
            if (e.Button == MouseButtons.Right) ShowNativeMenu();
        }

        private void ShowNativeMenu()
        {
            if (_events == null) return;
            bool startupEnabled = false;
            bool startupAvailable = true;
            try { startupEnabled = StartupRegistration.IsEnabled; }
            catch (Exception ex)
            {
                startupAvailable = false;
                _tray.ShowBalloonTip(5000, "Despert", Strings.Format("ReadStartupError", ex.Message),
                    ToolTipIcon.Warning);
            }
            int action;
            try
            {
                action = NativeMenu.Show(_events.Handle, _session.IsActive, startupEnabled, startupAvailable);
            }
            catch (Exception ex)
            {
                // Error només d'UI: no aturar una sessió activa que funciona.
                if (_tray != null)
                    _tray.ShowBalloonTip(5000, "Despert", Strings.Format("MenuError", ex.Message), ToolTipIcon.Error);
                return;
            }

            // L'acció s'executa després que el menú s'hagi destruït.
            if (action == 1) ToggleActive();
            else if (action == 2) ExitThread();
            else if (action == 3 && startupAvailable) ChangeStartup(!startupEnabled);
            else if (action == 4) ShowAbout();
        }

        private void ChangeStartup(bool enabled)
        {
            try
            {
                StartupRegistration.SetEnabled(enabled, Application.ExecutablePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Strings.Format("WriteStartupError", ex.Message),
                    "Despert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void ShowAbout()
        {
            if (_aboutDialog != null)
            {
                _aboutDialog.Activate();
                return;
            }
            using (AboutDialog dialog = new AboutDialog())
            {
                _aboutDialog = dialog;
                try { dialog.ShowDialog(); }
                finally { _aboutDialog = null; }
            }
        }

        private void RefreshAppearance()
        {
            if (_tray == null || _disposed) return;
            bool dark = Appearance.TaskbarDark;
            Size iconSize = SystemInformation.SmallIconSize;
            if (dark == _taskbarDark && iconSize == _iconSize) return;

            Icon inactive = null;
            Icon active = null;
            try
            {
                inactive = LoadIcon(dark ? "Despert.Inactive.Dark.ico" : "Despert.Inactive.Light.ico");
                try
                {
                    active = LoadIcon(dark ? "Despert.Active.Dark.ico" : "Despert.Active.Light.ico");
                }
                catch
                {
                    if (inactive != null) inactive.Dispose(); // neteja parcial de la segona càrrega
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (_tray != null)
                    _tray.ShowBalloonTip(5000, "Despert", Strings.Format("IconError", ex.Message),
                        ToolTipIcon.Warning);
                return;
            }

            Icon oldInactive = _iconInactive;
            Icon oldActive = _iconActive;
            _taskbarDark = dark;
            _iconSize = iconSize;
            _iconInactive = inactive;
            _iconActive = active;
            UpdateStatus();            // abans de disposar les icones velles
            if (oldInactive != null) oldInactive.Dispose();
            if (oldActive != null) oldActive.Dispose();
        }

        private void ToggleActive()
        {
            if (_session.IsActive)
            {
                Deactivate();
                return;
            }
            _errorShown = false; _lastError = null;
            try
            {
                _session.Start(_run.ElapsedMilliseconds, 0); // indefinit
                _paused = _events.Paused;
                if (!_paused) Native.HoldPower();
                UpdateStatus();
            }
            catch (Exception ex) { Fail(ex); }
        }

        private void Deactivate()
        {
            _session.Stop();
            ReleaseEffects();
            UpdateStatus();
        }

        // Allibera energia sense tocar la intenció activa de Session.
        private void ReleaseEffects()
        {
            try { Native.ReleasePower(); }
            catch { /* neteja de sortida: no propagar */ }
        }

        private void OnPauseChanged()
        {
            ApplyPause();
        }

        // Aplica HoldPower/ReleasePower només si canvia la pausa efectiva.
        private void ApplyPause()
        {
            bool paused = _events.Paused;
            if (paused == _paused) return;
            _paused = paused;
            if (!_session.IsActive) return;
            if (paused)
            {
                try { Native.ReleasePower(); }
                catch (Exception ex) { Fail(ex); }
            }
            else
            {
                try { Native.HoldPower(); }
                catch (Exception ex) { Fail(ex); return; }
                // reiniciar interval en reprendre efectes
                _session.RestartInterval(_run.ElapsedMilliseconds);
            }
        }

        private void UpdateStatus()
        {
            if (_tray == null) return;
            bool active = _session.IsActive;
            if (_lastError != null)
            {
                _tray.Icon = _iconInactive;
                _tray.Text = Strings.Get("StatusError");
            }
            else
            {
                _tray.Icon = active ? _iconActive : _iconInactive;
                _tray.Text = Strings.Get(active ? "StatusOn" : "StatusOff");
            }
        }

        private void Fail(Exception ex)
        {
            _lastError = ex.Message;
            _session.Stop();
            try { Native.ReleasePower(); } catch { /* neteja */ }
            UpdateStatus();
            if (!_errorShown && _tray != null)
            {
                _errorShown = true;
                _tray.ShowBalloonTip(5000, "Despert", Strings.Format("Error", _lastError),
                    ToolTipIcon.Error);
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_session.IsActive || _paused) return;
            try
            {
                long now = _run.ElapsedMilliseconds;
                uint idle = Native.IdleMs();
                bool held = Native.ButtonsHeld();
                // Revalidar just abans d'injectar.
                if (!_session.PulseDue(now, idle, held, true)) return;
                idle = Native.IdleMs();
                if (!_session.PulseDue(_run.ElapsedMilliseconds, idle, Native.ButtonsHeld(), true)) return;

                Native.Pulse(_session.Mode);
                _session.RecordPulse(_run.ElapsedMilliseconds);
            }
            catch (Exception ex) { Fail(ex); }
        }

        protected override void ExitThreadCore()
        {
            Dispose();
            base.ExitThreadCore();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                try
                {
                    _session.Stop();
                    Native.ReleasePower();
                }
                catch { /* neteja de sortida */ }
                if (_timer != null) { _timer.Stop(); _timer.Dispose(); }
                if (_aboutDialog != null) _aboutDialog.Close();
                if (_events != null) _events.Dispose();
                if (_tray != null)
                {
                    _tray.Visible = false;
                    _tray.Dispose();          // abans que les icones
                }
                if (_iconInactive != null) _iconInactive.Dispose();
                if (_iconActive != null) _iconActive.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal static class Program
    {
        [STAThread]
        private static int Main()
        {
            bool created;
            using (Mutex mutex = new Mutex(false, @"Local\Despert", out created))
            {
                if (!created)
                {
                    MessageBox.Show(Strings.Get("AlreadyRunning"), "Despert",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
                    using (DespertContext context = new DespertContext()) Application.Run(context);
                    return 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Strings.Format("Stopped", ex.Message), "Despert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }
                finally { try { Native.ReleasePower(); } catch { /* neteja final */ } }
            }
        }
    }
}
