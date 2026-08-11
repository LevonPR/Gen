using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.UI
{
    public class TutorialDirector : MonoBehaviour
    {
        int _step;
        float _timer;
        bool _done;
        readonly string[] _steps =
        {
            "Drag left stick or use WASD to swim",
            "Eat glowing food to gain biomass & ATP",
            "Hold Boost (Q) to dash — costs ATP",
            "Open Evolve (EVO / Tab) and unlock Oscillator",
            "Explore outward: Tide Pool → Midwater → Vent"
        };

        void Start()
        {
            _done = SaveSystem.LoadMeta().tutorialDone;
            _timer = 4.5f;
        }

        void Update()
        {
            if (_done) return;
            if (GameFlow.Instance != null && GameFlow.Instance.Screen != AppScreen.Playing) return;

            _timer -= Time.unscaledDeltaTime;
            if (_timer > 0f) return;
            _step++;
            if (_step >= _steps.Length)
            {
                _done = true;
                SaveSystem.SetTutorialDone(true);
                return;
            }

            _timer = 4.5f;
            GameEvents.RaiseToast(_steps[_step]);
        }

        public void Draw(GUIStyle panel, GUIStyle label)
        {
            if (_done || _step >= _steps.Length) return;
            if (GameFlow.Instance != null && GameFlow.Instance.Screen != AppScreen.Playing) return;
            var rect = new Rect(Screen.width * 0.5f - 220f, Screen.height - 150f, 440f, 48f);
            GUI.Box(rect, GUIContent.none, panel);
            GUI.Label(new Rect(rect.x + 12, rect.y + 12, rect.width - 24, 28), _steps[_step], label);
        }

        public void Skip()
        {
            _done = true;
            SaveSystem.SetTutorialDone(true);
        }
    }
}
