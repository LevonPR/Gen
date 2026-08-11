using System;

namespace MicroEvolution.Core
{
    public static class GameEvents
    {
        public static event Action StateChanged;
        public static event Action ObjectivesChanged;
        public static event Action<string> Toast;
        public static event Action Evolve;
        public static event Action<float> PlayerHurt;
        public static event Action AteFood;
        public static event Action KillPulse;

        public static void RaiseStateChanged() => StateChanged?.Invoke();
        public static void RaiseObjectivesChanged() => ObjectivesChanged?.Invoke();
        public static void RaiseToast(string message) => Toast?.Invoke(message);
        public static void RaiseEvolve() => Evolve?.Invoke();
        public static void RaisePlayerHurt(float amount) => PlayerHurt?.Invoke(amount);
        public static void RaiseAteFood() => AteFood?.Invoke();
        public static void RaiseKillPulse() => KillPulse?.Invoke();
    }
}
