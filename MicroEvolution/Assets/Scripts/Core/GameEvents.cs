using System;

namespace MicroEvolution.Core
{
    public static class GameEvents
    {
        public static event Action StateChanged;
        public static event Action ObjectivesChanged;
        public static event Action<string> Toast;

        public static void RaiseStateChanged() => StateChanged?.Invoke();
        public static void RaiseObjectivesChanged() => ObjectivesChanged?.Invoke();
        public static void RaiseToast(string message) => Toast?.Invoke(message);
    }
}
