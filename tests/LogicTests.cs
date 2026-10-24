// Despert - LogicTests.cs
// Proves console de la lògica pura. Cap Win32, cap UI, cap entrada real,
// cap canvi d'energia. Llicència MIT.
using System;
using Despert;

namespace DespertTests
{
    internal static class LogicTests
    {
        private static int _pass;
        private static int _fail;

        private static void Check(string name, bool cond)
        {
            if (cond) { _pass++; Console.WriteLine("PASS " + name); }
            else { _fail++; Console.WriteLine("FAIL " + name); }
        }

        public static int Main()
        {
            ApagadaInicial();
            GatingInactivitat();
            PeriodeEntreImpulsos();
            Expiracio();
            BotoPremut();
            AcBateria();
            PowerOnly();
            ReiniciSessio();
            WrapUint();
            LastFutur();
            ValidacioConfig();
            CanviMode();

            Console.WriteLine("Total PASS: " + _pass + "  FAIL: " + _fail);
            return _fail == 0 ? 0 : 1;
        }

        private static void ApagadaInicial()
        {
            Session s = new Session();
            Check("apagada: no actiu", !s.IsActive);
            Check("apagada: PulseDue fals", !s.PulseDue(0, 999999, false, true));
        }

        private static void GatingInactivitat()
        {
            Session s = new Session(); // IdleSeconds 45
            s.Start(0, 60);
            Check("idle curt: no impuls",
                !s.PulseDue(45000, 44u * 1000u, false, true));
            Check("inici: espera malgrat inactivitat anterior",
                !s.PulseDue(44999, 999999, false, true));
            Check("idle exacte: impuls",
                s.PulseDue(45000, 45u * 1000u, false, true));
            Check("entrada recent de qualsevol origen ajorna impuls",
                !s.PulseDue(90000, 1000, false, true));
        }

        private static void PeriodeEntreImpulsos()
        {
            Session s = new Session();
            s.Start(0, 60);
            Check("primer impuls degut", s.PulseDue(45000, 60000, false, true));
            s.RecordPulse(45000);
            Check("interval conservador: no immediat",
                !s.PulseDue(45000, 60000, false, true));
            Check("interval complet: encara no", !s.PulseDue(89999, 60000, false, true));
            Check("interval complet: limit exacte", s.PulseDue(90000, 60000, false, true));
        }

        private static void Expiracio()
        {
            Session s = new Session();
            s.Start(1000, 30); // 30 min = 1800000 ms
            long end = s.EndsAtMs;
            Check("expiracio: abans no", !s.IsExpired(end - 1));
            Check("expiracio: limit exacte", s.IsExpired(end));
            Check("expiracio: PulseDue fals", !s.PulseDue(end, 999999, false, true));
        }

        private static void BotoPremut()
        {
            Session s = new Session();
            s.Start(0, 60);
            Check("boto premut: no impuls",
                !s.PulseDue(45000, 999999, true, true));
        }

        private static void AcBateria()
        {
            Session s = new Session(); // AcOnly true
            s.Start(0, 60);
            Check("bateria amb AcOnly: no impuls",
                !s.PulseDue(45000, 999999, false, false));
            Session s2 = new Session();
            s2.SetAcOnly(false);
            s2.Start(0, 60);
            Check("bateria sense AcOnly: impuls",
                s2.PulseDue(45000, 999999, false, false));
        }

        private static void PowerOnly()
        {
            Session s = new Session();
            s.SetMode(KeepMode.PowerOnly);
            s.Start(0, 60);
            Check("poweronly: mai impuls",
                !s.PulseDue(45000, 999999, false, true));
        }

        private static void ReiniciSessio()
        {
            Session s = new Session();
            s.Start(0, 60);
            s.RecordPulse(0);
            s.Stop();
            Check("stop: inactiu", !s.IsActive);
            s.Start(100000, 60);
            Check("nou inici: no immediat",
                !s.PulseDue(100000, 60000, false, true));
            Check("nou inici: espera completa",
                s.PulseDue(145000, 60000, false, true));
        }

        private static void WrapUint()
        {
            uint now = 10u;
            uint last = uint.MaxValue - 9u; // wrap: han passat 20 ms
            Check("wrap uint", Session.IdleMilliseconds(now, last) == 20u);
            Check("normal", Session.IdleMilliseconds(5000u, 4000u) == 1000u);
        }

        private static void LastFutur()
        {
            Check("last lleugerament futur -> 0",
                Session.IdleMilliseconds(1000u, 5000u) == 0u);
            Check("last futur a traves del wrap -> 0",
                Session.IdleMilliseconds(uint.MaxValue - 10, 10) == 0u);
        }

        private static void ValidacioConfig()
        {
            Session s = new Session();
            Check("idle per defecte 45", s.IdleSeconds == 45);
            bool ok = false;
            try { s.SetIdleSeconds(15); } catch (ArgumentOutOfRangeException) { ok = true; }
            Check("idle 15 rebutjat", ok);
            s.SetIdleSeconds(30);
            Check("idle 30 acceptat", s.IdleSeconds == 30);
            bool ok2 = false;
            try { s.Start(0, 45); } catch (ArgumentOutOfRangeException) { ok2 = true; }
            Check("durada 45 rebutjada", ok2);
            s.Start(0, 0);
            Check("indefinit sense EndsAt", s.EndsAtMs == 0 && s.IsActive);
            Check("indefinit no caduca", !s.IsExpired(long.MaxValue));
            bool invalidMode = false;
            try { s.SetMode((KeepMode)999); } catch (ArgumentOutOfRangeException) { invalidMode = true; }
            Check("mode desconegut rebutjat", invalidMode);
        }

        private static void CanviMode()
        {
            Session s = new Session();
            s.Start(1000, 30);
            long deadline = s.EndsAtMs;
            s.SetMode(KeepMode.MinimalMove);
            s.RestartInterval(60000);
            Check("canvi mode: sense impuls immediat", !s.PulseDue(60000, 999999, false, true));
            Check("canvi mode: espera completa", s.PulseDue(105000, 999999, false, true));
            Check("canvi mode: conserva expiracio", s.EndsAtMs == deadline);
            Check("temps enrere: espera conservadora", !s.PulseDue(59999, 999999, false, true));
        }
    }
}
