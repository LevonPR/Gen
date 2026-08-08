using UnityEngine;

namespace MicroEvolution.UI
{
    /// <summary>
    /// Legacy IMGUI HUD — replaced by <see cref="CinematicHud"/>.
    /// Kept so older scene references do not break; does nothing at runtime.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        void Awake()
        {
            // Prefer cinematic canvas HUD if present.
            if (FindObjectOfType<CinematicHud>() != null)
                enabled = false;
        }
    }
}
