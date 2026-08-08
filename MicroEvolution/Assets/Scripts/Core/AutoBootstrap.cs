using UnityEngine;

namespace MicroEvolution.Core
{
    /// <summary>
    /// Starts the game automatically when any scene is played,
    /// so you don't need to wire a scene manually in the Editor.
    /// </summary>
    public static class AutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureGame()
        {
            if (Object.FindObjectOfType<GameBootstrap>() != null) return;
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }
    }
}
