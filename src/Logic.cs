// Despert - Logic.cs
// Lògica pura i testable de la sessió anti-inactivitat.
// Llicència MIT. Temps monotònic (Stopwatch/mil·lisegons), mai DateTime per expiració.
// Nota: GetLastInputInfo NO detecta exclusivament entrada física; inclou impulsos
// propis injectats per la mateixa aplicació.
using System;

namespace Despert
{
    public enum KeepMode
    {
        PowerOnly,
        ZeroMove,
        MinimalMove
    }

    public class Session
    {
        private long _durationMs; // 0 = indefinit
        private long _lastPulseMs;

        public Session()
        {
            IsActive = false;
            EndsAtMs = 0;
            IdleSeconds = 45;
            Mode = KeepMode.ZeroMove;
            AcOnly = true;
        }

        public bool IsActive { get; private set; }
        public long EndsAtMs { get; private set; }
        public int IdleSeconds { get; private set; }
        public KeepMode Mode { get; private set; }
        public bool AcOnly { get; private set; }

        public void SetIdleSeconds(int seconds)
        {
            if (seconds != 30 && seconds != 45 && seconds != 60)
                throw new ArgumentOutOfRangeException("seconds", "Només 30/45/60.");
            IdleSeconds = seconds;
        }

        public void SetMode(KeepMode mode)
        {
            if (!Enum.IsDefined(typeof(KeepMode), mode))
                throw new ArgumentOutOfRangeException("mode");
            Mode = mode;
        }

        public void SetAcOnly(bool value)
        {
            AcOnly = value;
        }

        public void Start(long nowMs, int durationMinutes)
        {
            if (durationMinutes != 0 && durationMinutes != 30 &&
                durationMinutes != 60 && durationMinutes != 120)
                throw new ArgumentOutOfRangeException("durationMinutes");
            IsActive = true;
            _durationMs = durationMinutes > 0
                ? (long)durationMinutes * 60L * 1000L
                : 0L;
            EndsAtMs = _durationMs > 0 ? nowMs + _durationMs : 0;
            RestartInterval(nowMs);
        }

        public void Stop()
        {
            IsActive = false;
        }

        public bool IsExpired(long nowMs)
        {
            return IsActive && _durationMs > 0 && nowMs >= EndsAtMs;
        }

        // Reinicia l'interval conservador des de l'instant actual.
        public void RestartInterval(long nowMs)
        {
            _lastPulseMs = nowMs;
        }

        // Decideix si toca injectar un impuls. No produeix cap efecte lateral.
        public bool PulseDue(long nowMs, uint idleMs, bool buttonHeld, bool onAc)
        {
            if (!IsActive) return false;
            if (_durationMs > 0 && nowMs >= EndsAtMs) return false;
            if (Mode == KeepMode.PowerOnly) return false;
            if (buttonHeld) return false;
            if (AcOnly && !onAc) return false;
            uint idleNeeded = (uint)IdleSeconds * 1000u;
            if (idleMs < idleNeeded) return false;
            return nowMs >= _lastPulseMs && nowMs - _lastPulseMs >= idleNeeded;
        }

        public void RecordPulse(long nowMs)
        {
            if (!IsActive) return;
            _lastPulseMs = nowMs;
        }

        // Resta amb wrap uint. L'altra meitat del rang és ambigua (futur o
        // més de 24,8 dies inactiu): no activar mai per una inactivitat gegant falsa.
        public static uint IdleMilliseconds(uint now, uint last)
        {
            unchecked
            {
                uint elapsed = now - last;
                return elapsed > int.MaxValue ? 0u : elapsed;
            }
        }
    }
}
