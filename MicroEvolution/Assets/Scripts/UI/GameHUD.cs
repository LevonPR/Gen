using MicroEvolution.Audio;
using MicroEvolution.Core;
using MicroEvolution.Evolution;
using MicroEvolution.InputSystem;
using MicroEvolution.Player;
using MicroEvolution.World;
using UnityEngine;

namespace MicroEvolution.UI
{
    public class GameHUD : MonoBehaviour
    {
        PlayerController _player;
        TouchControls _touch;
        TutorialDirector _tutorial;
        bool _evolveOpen;
        string _toast = string.Empty;
        float _toastTimer;
        GUIStyle _panel, _label, _title, _button, _bigTitle;
        Texture2D _white;
        bool _stylesReady;

        public void Bind(PlayerController player, TouchControls touch, TutorialDirector tutorial)
        {
            _player = player;
            _touch = touch;
            _tutorial = tutorial;
        }

        void OnEnable() => GameEvents.Toast += OnToast;
        void OnDisable() => GameEvents.Toast -= OnToast;
        void OnToast(string message) { _toast = message; _toastTimer = 3f; }

        void Update()
        {
            if (_toastTimer > 0f) _toastTimer -= Time.unscaledDeltaTime;
            if (GameInput.Instance != null && GameInput.Instance.EvolvePanelPressed)
                _evolveOpen = !_evolveOpen;
            if (GameInput.Instance != null && GameInput.Instance.PausePressed)
                GameFlow.Instance?.TogglePause();
        }

        void OnGUI()
        {
            EnsureStyles();
            var flow = GameFlow.Instance;
            if (flow == null) return;

            switch (flow.Screen)
            {
                case AppScreen.MainMenu:
                    DrawMainMenu();
                    break;
                case AppScreen.Playing:
                    DrawPlayHud();
                    break;
                case AppScreen.Paused:
                    DrawPlayHud();
                    DrawPauseOverlay();
                    break;
                case AppScreen.Victory:
                    DrawPlayHud();
                    DrawEndCard("CELL STAGE COMPLETE", "Thermal vents conquered. New Run keeps meta bonuses.", true);
                    break;
                case AppScreen.GameOver:
                    DrawEndCard("COLONY COLLAPSED", "Population reached zero. Try a new run.", false);
                    break;
            }
        }

        void DrawMainMenu()
        {
            DrawBackdrop();
            var meta = SaveSystem.LoadMeta();
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.22f, 440f, 420f));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label("MICROEVOLUTION", _bigTitle);
            GUILayout.Label("Spore-inspired Cell Stage", _label);
            GUILayout.Space(12);
            GUILayout.Label($"Best population: {meta.bestPopulation}", _label);
            GUILayout.Label($"Biome access: {meta.unlockedBiomeIndex + 1}/3", _label);
            GUILayout.Label($"Runs: {meta.totalRuns}", _label);
            GUILayout.Space(16);
            if (GUILayout.Button("START RUN", _button, GUILayout.Height(44)))
            {
                AudioDirector.Instance?.PlayUi();
                FindObjectOfType<GameBootstrap>()?.BeginRunFromMenu();
            }

            if (GUILayout.Button(meta.muted ? "Sound: Off" : "Sound: On", _button, GUILayout.Height(36)))
            {
                var muted = !(AudioDirector.Instance?.Muted ?? meta.muted);
                AudioDirector.Instance?.SetMuted(muted);
            }

            if (GUILayout.Button(meta.reducedParticles ? "Particles: Low" : "Particles: Full", _button, GUILayout.Height(36)))
            {
                SaveSystem.SetReducedParticles(!meta.reducedParticles);
            }

            GUILayout.Space(8);
            GUILayout.Label("Android: left stick · Q boost · E chem · EVO shop", _label);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawPlayHud()
        {
            var state = GameState.Instance;
            if (state == null) return;

            DrawTopLeft(state);
            DrawObjectives(state);
            DrawBottomLeft(state);
            DrawHealth(state);
            DrawAbilities(state);
            DrawMinimap(state);
            DrawToast();
            _touch?.DrawGUI(_panel, _button);
            _tutorial?.Draw(_panel, _label);
            if (_evolveOpen) DrawEvolvePanel(state);

#if !UNITY_ANDROID
            GUI.Label(new Rect(18, Screen.height - 22, 1000, 20),
                "WASD · RMB steer · Q boost · E chem · Tab evolve · 1-9 buy · P pause", _label);
#endif
        }

        void DrawPauseOverlay()
        {
            DrawBackdrop();
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.3f, 360f, 240f));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label("PAUSED", _bigTitle);
            if (GUILayout.Button("Resume", _button, GUILayout.Height(40)))
                GameFlow.Instance.Resume();
            if (GUILayout.Button("Main Menu", _button, GUILayout.Height(40)))
                FindObjectOfType<GameBootstrap>()?.ReturnToMenu();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawEndCard(string title, string subtitle, bool victory)
        {
            DrawBackdrop();
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 240f, Screen.height * 0.28f, 480f, 260f));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label(title, _bigTitle);
            GUILayout.Label(subtitle, _label);
            GUILayout.Space(12);
            if (GUILayout.Button("New Run", _button, GUILayout.Height(40)))
                FindObjectOfType<GameBootstrap>()?.BeginRunFromMenu();
            if (GUILayout.Button("Main Menu", _button, GUILayout.Height(36)))
                FindObjectOfType<GameBootstrap>()?.ReturnToMenu();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawEvolvePanel(GameState state)
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.5f - 200f, 80f, 400f, Screen.height - 160f));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label($"EVOLVE  ·  {state.EvolutionPoints} points", _title);
            foreach (var part in PartCatalog.All)
            {
                var owned = part.IsOwned(state);
                GUI.enabled = !owned && state.EvolutionPoints >= part.Cost;
                var label = owned ? $"{part.Name} ✓" : $"{part.Name} ({part.Cost}) — {part.Description}";
                if (GUILayout.Button(label, _button, GUILayout.Height(32)))
                {
                    PartCatalog.TryBuy(part.Id);
                    AudioDirector.Instance?.PlayUi();
                }

                GUI.enabled = true;
            }

            if (GUILayout.Button("Close", _button, GUILayout.Height(34)))
                _evolveOpen = false;
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawTopLeft(GameState state)
        {
            GUILayout.BeginArea(new Rect(14, 12, 260, 100));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label("ATP", _title);
            DrawBar(state.Atp / Mathf.Max(0.01f, state.AtpMax), new Color(0.25f, 0.75f, 1f));
            GUILayout.Label($"{state.Atp:0}/{state.AtpMax:0}   Evo {state.EvolutionPoints}", _label);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawObjectives(GameState state)
        {
            GUILayout.BeginArea(new Rect(14, 120, 300, 130));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label("OBJECTIVES", _title);
            GUILayout.Label($"{Mark(state.PopulationGoalMet)} Population {state.Population}/{GameConfig.PopulationGoal}", _label);
            GUILayout.Label($"{Mark(state.OscillatorGoalMet)} Evolve Oscillator", _label);
            GUILayout.Label($"{Mark(state.BiomeGoalMet)} Reach Thermal Vent", _label);
            var biome = BiomeSystem.Instance != null ? BiomeSystem.BiomeName(BiomeSystem.Instance.ActiveBiome) : "";
            GUILayout.Label(biome, _label);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawBottomLeft(GameState state)
        {
            GUILayout.BeginArea(new Rect(14, Screen.height - 210f, 200, 70));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label($"Pop {state.Population}   Biomass {FormatBiomass(state.Biomass)}", _label);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawHealth(GameState state)
        {
            var width = 360f;
            var x = (Screen.width - width) * 0.5f;
            GUILayout.BeginArea(new Rect(x, Screen.height - 78f, width, 54f));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label("HEALTH", _title);
            DrawBar(state.Health / Mathf.Max(0.01f, state.MaxHealth), new Color(0.35f, 0.9f, 0.45f));
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawAbilities(GameState state)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 250f, Screen.height - 280f, 230f, 90f));
            GUILayout.BeginVertical(_panel);
            var chem = _player == null || _player.ChemCooldownRemaining <= 0f ? "Ready" : $"{_player.ChemCooldownRemaining:0.0}s";
            GUILayout.Label($"CHEM [{chem}]", _title);
            DrawBar(1f - (_player?.ChemCooldownNormalized ?? 0f), new Color(0.3f, 0.8f, 1f));
            GUILayout.Label($"BOOST {(_player != null && _player.Boosting ? "ON" : "off")}", _label);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawMinimap(GameState state)
        {
            const float size = 120f;
            var rect = new Rect(Screen.width - size - 16f, 16f, size, size);
            GUI.Box(rect, GUIContent.none, _panel);
            var center = rect.center;
            foreach (var cell in CellRegistry.All)
            {
                if (cell == null) continue;
                var p = cell.transform.position;
                var nx = center.x + (p.x / GameConfig.WorldRadius) * (size * 0.4f);
                var ny = center.y - (p.y / GameConfig.WorldRadius) * (size * 0.4f);
                DrawDot(new Vector2(nx, ny), DotColor(cell.Faction), cell.IsPlayer ? 5f : 3f);
            }
        }

        void DrawToast()
        {
            if (_toastTimer <= 0f || string.IsNullOrEmpty(_toast)) return;
            var width = 520f;
            GUI.Box(new Rect((Screen.width - width) * 0.5f, 20f, width, 40f), _toast, _panel);
        }

        void DrawBackdrop()
        {
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _white);
            GUI.color = prev;
        }

        void DrawBar(float fill01, Color color)
        {
            var rect = GUILayoutUtility.GetRect(16, 14, GUILayout.ExpandWidth(true));
            GUI.Box(rect, GUIContent.none, _panel);
            var fill = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * Mathf.Clamp01(fill01), rect.height - 4);
            var old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(fill, _white);
            GUI.color = old;
        }

        void DrawDot(Vector2 pos, Color color, float radius)
        {
            var old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(pos.x - radius, pos.y - radius, radius * 2f, radius * 2f), _white);
            GUI.color = old;
        }

        static string Mark(bool done) => done ? "✓" : "○";
        static string FormatBiomass(float value) => value >= 1000f ? $"{value / 1000f:0.0}K" : $"{value:0}";

        static Color DotColor(Faction faction)
        {
            switch (faction)
            {
                case Faction.Player: return new Color(0.4f, 0.9f, 1f);
                case Faction.Ally: return new Color(0.4f, 1f, 0.7f);
                case Faction.Prey: return new Color(1f, 0.9f, 0.35f);
                case Faction.Predator: return new Color(1f, 0.35f, 0.35f);
                default: return Color.white;
            }
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            _white = Texture2D.whiteTexture;
            _panel = new GUIStyle(GUI.skin.box);
            _panel.normal.background = MakeTex(new Color(0.02f, 0.08f, 0.12f, 0.78f));
            _panel.padding = new RectOffset(12, 12, 10, 10);
            _label = new GUIStyle(GUI.skin.label) { fontSize = 13, normal = { textColor = new Color(0.82f, 0.93f, 1f) } };
            _title = new GUIStyle(_label) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.55f, 0.9f, 1f) } };
            _bigTitle = new GUIStyle(_title) { fontSize = 26, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.65f, 0.95f, 1f) } };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
            _button.normal.background = MakeTex(new Color(0.08f, 0.25f, 0.35f, 0.95f));
            _button.hover.background = MakeTex(new Color(0.12f, 0.38f, 0.5f, 0.95f));
            _button.active.background = MakeTex(new Color(0.16f, 0.48f, 0.58f, 0.95f));
            _button.normal.textColor = _button.hover.textColor = _button.active.textColor = Color.white;
            _stylesReady = true;
        }

        static Texture2D MakeTex(Color color)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, color);
            t.Apply();
            return t;
        }
    }
}
