using MicroEvolution.Core;
using MicroEvolution.Evolution;
using MicroEvolution.Player;
using UnityEngine;

namespace MicroEvolution.UI
{
    public class GameHUD : MonoBehaviour
    {
        PlayerController _player;
        string _toast = string.Empty;
        float _toastTimer;
        GUIStyle _panel;
        GUIStyle _label;
        GUIStyle _title;
        GUIStyle _barBg;
        GUIStyle _barFill;
        GUIStyle _button;
        Texture2D _white;
        bool _stylesReady;

        public void Bind(PlayerController player) => _player = player;

        void OnEnable() => GameEvents.Toast += OnToast;
        void OnDisable() => GameEvents.Toast -= OnToast;

        void OnToast(string message)
        {
            _toast = message;
            _toastTimer = 3.2f;
        }

        void Update()
        {
            if (_toastTimer > 0f) _toastTimer -= Time.deltaTime;
        }

        void OnGUI()
        {
            EnsureStyles();
            if (GameState.Instance == null) return;

            var state = GameState.Instance;
            DrawTopLeft(state);
            DrawObjectives(state);
            DrawBottomLeft(state);
            DrawHealthAndHotbar(state);
            DrawAbilities(state);
            DrawMinimap(state);
            DrawToast();
            DrawHelp();
            if (state.Victory) DrawCenteredBanner("CELL STAGE COMPLETE", "Keep exploring or evolve further parts (1-4)");
            if (state.PlayerDead) DrawCenteredBanner("CELL DESTROYED", "Press R to respawn from your colony");
        }

        void DrawTopLeft(GameState state)
        {
            GUILayout.BeginArea(new Rect(18, 16, 280, 110));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label("ATP", _title);
            DrawBar(state.Atp / Mathf.Max(0.01f, state.AtpMax), new Color(0.25f, 0.75f, 1f));
            GUILayout.Label($"{state.Atp:0}/{state.AtpMax:0}", _label);
            GUILayout.Space(6);
            GUILayout.Label($"Evolution Points  {state.EvolutionPoints}", _title);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawObjectives(GameState state)
        {
            GUILayout.BeginArea(new Rect(18, 140, 300, 120));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label("OBJECTIVES", _title);
            var popDone = state.PopulationGoalMet ? "✓" : "○";
            var oscDone = state.OscillatorGoalMet ? "✓" : "○";
            GUILayout.Label($"{popDone} Reach population 150  ({state.Population}/{GameConfig.PopulationGoal})", _label);
            GUILayout.Label($"{oscDone} Evolve Oscillator  ({(state.HasOscillator ? 1 : 0)}/1)", _label);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawBottomLeft(GameState state)
        {
            GUILayout.BeginArea(new Rect(18, Screen.height - 120, 220, 100));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label($"Population  {state.Population}", _label);
            GUILayout.Label($"Biomass  {FormatBiomass(state.Biomass)}", _label);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawHealthAndHotbar(GameState state)
        {
            var width = 420f;
            var x = (Screen.width - width) * 0.5f;
            GUILayout.BeginArea(new Rect(x, Screen.height - 92, width, 80));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label("HEALTH", _title);
            DrawBar(state.Health / Mathf.Max(0.01f, state.MaxHealth), new Color(0.35f, 0.9f, 0.45f));
            GUILayout.BeginHorizontal();
            DrawSlot("1 Osc", state.HasOscillator);
            DrawSlot("2 Spike", state.HasSpikes);
            DrawSlot("3 Memb", state.HasMembrane);
            DrawSlot("4 Chem", state.HasChemosynthesisUpgrade);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawAbilities(GameState state)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 150, 240, 130));
            GUILayout.BeginVertical(_panel);
            var chemReady = _player == null || _player.ChemCooldownRemaining <= 0f;
            var chemText = chemReady ? "Ready" : $"{_player.ChemCooldownRemaining:0.0}s";
            GUILayout.Label($"CHEMOSYNTHESIS  [{chemText}]", _title);
            GUILayout.Label("Press E", _label);
            GUILayout.Space(8);
            GUILayout.Label("SPEED BOOST  [Q]", _title);
            var boostFill = (_player != null && _player.Boosting) ? 1f : state.Atp / Mathf.Max(0.01f, state.AtpMax);
            DrawBar(boostFill, new Color(0.3f, 0.65f, 1f));
            GUILayout.EndVertical();
            GUILayout.EndArea();

            // Evolution purchase buttons
            GUILayout.BeginArea(new Rect(Screen.width - 260, 160, 240, 210));
            GUILayout.BeginVertical(_panel);
            GUILayout.Label("EVOLVE", _title);
            DrawBuyButton($"Oscillator ({EvolutionShop.OscillatorCost})", !state.HasOscillator, EvolutionShop.TryBuyOscillator);
            DrawBuyButton($"Spikes ({EvolutionShop.SpikesCost})", !state.HasSpikes, EvolutionShop.TryBuySpikes);
            DrawBuyButton($"Membrane ({EvolutionShop.MembraneCost})", !state.HasMembrane, EvolutionShop.TryBuyMembrane);
            DrawBuyButton($"Chemosynthesis ({EvolutionShop.ChemosynthesisCost})", !state.HasChemosynthesisUpgrade, EvolutionShop.TryBuyChemosynthesis);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawMinimap(GameState state)
        {
            const float size = 140f;
            var rect = new Rect(Screen.width - size - 20, 20, size, size);
            GUI.Box(rect, GUIContent.none, _panel);

            var center = rect.center;
            var world = GameConfig.WorldRadius;
            foreach (var cell in CellRegistry.All)
            {
                if (cell == null) continue;
                var p = cell.transform.position;
                var nx = center.x + (p.x / world) * (size * 0.42f);
                var ny = center.y - (p.y / world) * (size * 0.42f);
                var color = DotColor(cell.Faction);
                DrawDot(new Vector2(nx, ny), color, cell.IsPlayer ? 5f : 3f);
            }

            // Food dots (sample)
            var foods = FindObjectsOfType<World.FoodPellet>();
            var count = Mathf.Min(foods.Length, 40);
            for (var i = 0; i < count; i++)
            {
                var p = foods[i].transform.position;
                var nx = center.x + (p.x / world) * (size * 0.42f);
                var ny = center.y - (p.y / world) * (size * 0.42f);
                DrawDot(new Vector2(nx, ny), new Color(0.6f, 1f, 0.4f, 0.7f), 2f);
            }

            GUI.Label(new Rect(rect.x + 8, rect.yMax - 22, 100, 20), "MAP", _label);
        }

        void DrawToast()
        {
            if (_toastTimer <= 0f || string.IsNullOrEmpty(_toast)) return;
            var width = 520f;
            var rect = new Rect((Screen.width - width) * 0.5f, 24, width, 40);
            GUI.Box(rect, _toast, _panel);
        }

        void DrawHelp()
        {
            GUI.Label(new Rect(18, Screen.height - 28, 900, 24),
                "WASD / Arrows move · Hold RMB swim to cursor · Q boost · E chemosynthesis · 1-4 evolve · R respawn",
                _label);
        }

        void DrawCenteredBanner(string title, string subtitle)
        {
            var rect = new Rect(Screen.width * 0.5f - 240, Screen.height * 0.35f, 480, 90);
            GUI.Box(rect, GUIContent.none, _panel);
            GUI.Label(new Rect(rect.x + 20, rect.y + 16, rect.width - 40, 30), title, _title);
            GUI.Label(new Rect(rect.x + 20, rect.y + 48, rect.width - 40, 30), subtitle, _label);
        }

        void DrawBuyButton(string text, bool available, System.Func<bool> buy)
        {
            GUI.enabled = available;
            if (GUILayout.Button(available ? text : $"{text} ✓", _button, GUILayout.Height(28)))
                buy();
            GUI.enabled = true;
        }

        void DrawSlot(string text, bool owned)
        {
            var c = GUI.backgroundColor;
            GUI.backgroundColor = owned ? new Color(0.3f, 0.8f, 0.6f) : new Color(0.15f, 0.25f, 0.35f);
            GUILayout.Box(text, _button, GUILayout.Width(90), GUILayout.Height(28));
            GUI.backgroundColor = c;
        }

        void DrawBar(float fill01, Color color)
        {
            var rect = GUILayoutUtility.GetRect(18, 16, GUILayout.ExpandWidth(true));
            GUI.Box(rect, GUIContent.none, _barBg);
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

        static string FormatBiomass(float value)
        {
            if (value >= 1000f) return $"{value / 1000f:0.0}K";
            return $"{value:0}";
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            _white = Texture2D.whiteTexture;

            _panel = new GUIStyle(GUI.skin.box);
            _panel.normal.background = MakeTex(new Color(0.02f, 0.08f, 0.12f, 0.72f));
            _panel.padding = new RectOffset(12, 12, 10, 10);

            _label = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.8f, 0.92f, 1f) }
            };

            _title = new GUIStyle(_label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.55f, 0.9f, 1f) }
            };

            _barBg = new GUIStyle(GUI.skin.box);
            _barBg.normal.background = MakeTex(new Color(0.05f, 0.12f, 0.18f, 0.9f));

            _button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            _button.normal.background = MakeTex(new Color(0.08f, 0.22f, 0.32f, 0.95f));
            _button.hover.background = MakeTex(new Color(0.12f, 0.35f, 0.48f, 0.95f));
            _button.active.background = MakeTex(new Color(0.16f, 0.45f, 0.55f, 0.95f));
            _button.normal.textColor = Color.white;
            _button.hover.textColor = Color.white;
            _button.active.textColor = Color.white;

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
