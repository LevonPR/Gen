using MicroEvolution.Audio;
using MicroEvolution.Core;
using MicroEvolution.Evolution;
using MicroEvolution.InputSystem;
using MicroEvolution.Player;
using MicroEvolution.Visuals;
using MicroEvolution.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MicroEvolution.UI
{
    /// <summary>
    /// Mobile cinematic HUD matching Micronus / Evolve Cell Stage concept art.
    /// </summary>
    public class CinematicHud : MonoBehaviour
    {
        PlayerController _player;
        CellAppearance _playerLook;
        Canvas _canvas;
        GameObject _playRoot;
        GameObject _menuRoot;
        GameObject _evolveRoot;
        GameObject _customizeRoot;
        GameObject _overlayRoot;

        Text _title;
        Text _region;
        Text _objectives;
        Text _dna;
        Text _generation;
        Text _score;
        Text _healthText;
        Text _energyText;
        Text _toast;
        Text _overlayTitle;
        Text _overlayBody;
        Image _healthFill;
        Image _energyFill;
        Image _evoFill;
        Image _minimap;
        RawImage _minimapDots;
        Texture2D _radarTex;

        // Virtual stick (canvas)
        RectTransform _stickKnob;
        RectTransform _stickArea;
        bool _evolveOpen;
        bool _customizeOpen;
        float _toastTimer;
        TutorialDirector _tutorial;

        static readonly Color[] MembraneSwatches =
        {
            new Color(0.35f, 0.8f, 1f, 0.72f),
            new Color(0.4f, 1f, 0.65f, 0.72f),
            new Color(0.85f, 0.45f, 1f, 0.72f),
            new Color(1f, 0.7f, 0.35f, 0.72f),
            new Color(1f, 0.4f, 0.45f, 0.72f),
            new Color(0.55f, 0.9f, 1f, 0.72f),
        };

        public void Build()
        {
            EnsureEventSystem();
            var canvasGo = new GameObject("CinematicCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _menuRoot = BuildMainMenu(canvasGo.transform);
            _playRoot = BuildPlayHud(canvasGo.transform);
            _evolveRoot = BuildEvolveScreen(canvasGo.transform);
            _customizeRoot = BuildCustomizeScreen(canvasGo.transform);
            _overlayRoot = BuildOverlay(canvasGo.transform);

            _playRoot.SetActive(false);
            _evolveRoot.SetActive(false);
            _customizeRoot.SetActive(false);
            _overlayRoot.SetActive(false);
            _menuRoot.SetActive(true);

            var tutGo = new GameObject("Tutorial");
            tutGo.transform.SetParent(transform, false);
            _tutorial = tutGo.AddComponent<TutorialDirector>();
        }

        public void BindPlayer(PlayerController player)
        {
            _player = player;
            _playerLook = player != null ? player.GetComponent<CellAppearance>() : null;
        }

        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        GameObject BuildMainMenu(Transform parent)
        {
            var root = UiTheme.MakePanel(parent, "MainMenu", new Color(0.01f, 0.05f, 0.08f, 0.55f), true).gameObject;
            UiTheme.Stretch(root.transform);

            var card = UiTheme.MakePanel(root.transform, "Card", UiTheme.BgPanelSolid, true);
            UiTheme.SetAnchored(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 520));

            UiTheme.MakeText(card.transform, "MICROEVOLUTION", 40, UiTheme.AccentCyan, FontStyle.Bold, TextAnchor.MiddleCenter);
            var t1 = card.transform.Find("Text").GetComponent<Text>();
            UiTheme.SetAnchored(t1.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -36), new Vector2(480, 50));

            var sub = UiTheme.MakeText(card.transform, "CELL STAGE", 22, UiTheme.AccentGreen, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.SetAnchored(sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -78), new Vector2(480, 30));

            var blurb = UiTheme.MakeText(card.transform, "Cinematic microbial survival.\nEat · Evolve · Reproduce · Conquer biomes.", 16, UiTheme.TextDim, FontStyle.Normal, TextAnchor.MiddleCenter);
            UiTheme.SetAnchored(blurb.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -130), new Vector2(440, 60));

            var start = UiTheme.MakeButton(card.transform, "Start", UiTheme.AccentCyan * new Color(0.2f, 0.45f, 0.55f, 1f));
            UiTheme.SetAnchored(start.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(300, 64));
            var startLabel = UiTheme.MakeText(start.transform, "START RUN", 22, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.Stretch(startLabel);
            start.onClick.AddListener(() =>
            {
                AudioDirector.Instance?.PlayUi();
                FindObjectOfType<GameBootstrap>()?.BeginRunFromMenu();
            });

            var customize = UiTheme.MakeButton(card.transform, "Customize", UiTheme.HexSlot);
            UiTheme.SetAnchored(customize.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(300, 52));
            var cLabel = UiTheme.MakeText(customize.transform, "CUSTOMIZE CELL", 18, UiTheme.TextPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.Stretch(cLabel);
            customize.onClick.AddListener(() => OpenCustomize(true));

            return root;
        }

        GameObject BuildPlayHud(Transform parent)
        {
            var root = new GameObject("PlayHud");
            root.transform.SetParent(parent, false);
            UiTheme.Stretch(root.AddComponent<RectTransform>());

            // Top-left brand + objectives
            var left = UiTheme.MakePanel(root.transform, "LeftPanel", UiTheme.BgPanel);
            UiTheme.SetAnchored(left.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -24), new Vector2(360, 210));
            // Accent edge
            var edge = UiTheme.MakePanel(left.transform, "Edge", UiTheme.AccentCyan * new Color(1, 1, 1, 0.35f));
            UiTheme.SetAnchored(edge.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(3, 0));

            _title = UiTheme.MakeText(left.transform, "MicroEvolution", 22, UiTheme.AccentCyan, FontStyle.Bold);
            UiTheme.SetAnchored(_title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(-24, 28));
            _region = UiTheme.MakeText(left.transform, "Region: Eukaryotic Shoal", 14, UiTheme.TextDim);
            UiTheme.SetAnchored(_region.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -38), new Vector2(-24, 22));
            _objectives = UiTheme.MakeText(left.transform, "", 15, UiTheme.TextPrimary);
            _objectives.alignment = TextAnchor.UpperLeft;
            UiTheme.SetAnchored(_objectives.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(-28, -70));

            // Top center DNA / generation / score
            var top = UiTheme.MakePanel(root.transform, "TopCenter", UiTheme.BgPanel);
            UiTheme.SetAnchored(top.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -18), new Vector2(420, 78));
            _evoFill = UiTheme.MakeBarFill(top.transform, UiTheme.AccentDna);
            UiTheme.SetAnchored(_evoFill.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(-40, 12));
            _dna = UiTheme.MakeText(top.transform, "DNA 0", 20, UiTheme.AccentDna, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.SetAnchored(_dna.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 2), new Vector2(-20, 28));
            _generation = UiTheme.MakeText(top.transform, "GENERATION 1", 13, UiTheme.TextDim, FontStyle.Normal, TextAnchor.MiddleLeft);
            UiTheme.SetAnchored(_generation.rectTransform, new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(16, 8), new Vector2(160, 20));
            _score = UiTheme.MakeText(top.transform, "SCORE 0", 13, UiTheme.TextDim, FontStyle.Normal, TextAnchor.MiddleRight);
            UiTheme.SetAnchored(_score.rectTransform, new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-16, 8), new Vector2(160, 20));

            // Circular radar (mockup style)
            _minimap = UiTheme.MakePanel(root.transform, "Minimap", new Color(0.05f, 0.15f, 0.2f, 0.75f));
            _minimap.sprite = ProceduralSprites.Circle("radar-bg", Color.white, 128);
            UiTheme.SetAnchored(_minimap.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(150, 150));
            var radarRing = UiTheme.MakePanel(_minimap.transform, "Ring", UiTheme.AccentCyan * new Color(1, 1, 1, 0.45f));
            radarRing.sprite = ProceduralSprites.Ring("radar-ring", Color.white, 128, 0.08f);
            UiTheme.Stretch(radarRing);
            var radarGo = new GameObject("Radar");
            radarGo.transform.SetParent(_minimap.transform, false);
            _minimapDots = radarGo.AddComponent<RawImage>();
            _radarTex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            _radarTex.filterMode = FilterMode.Point;
            _minimapDots.texture = _radarTex;
            UiTheme.Stretch(_minimapDots);
            // Keep radar dots inside circle via mask-ish transparency already in texture clear color

            var gear = UiTheme.MakeButton(root.transform, "Pause", UiTheme.HexSlot);
            UiTheme.SetAnchored(gear.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-185, -28), new Vector2(48, 48));
            var gearTxt = UiTheme.MakeText(gear.transform, "II", 18, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.Stretch(gearTxt);
            gear.onClick.AddListener(() => GameFlow.Instance?.TogglePause());

            // Bottom vitals
            var vitals = UiTheme.MakePanel(root.transform, "Vitals", UiTheme.BgPanel);
            UiTheme.SetAnchored(vitals.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(520, 86));
            _healthText = UiTheme.MakeText(vitals.transform, "HEALTH", 12, UiTheme.AccentGreen, FontStyle.Bold);
            UiTheme.SetAnchored(_healthText.rectTransform, new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0, 1), new Vector2(16, -8), new Vector2(200, 18));
            var hBg = UiTheme.MakePanel(vitals.transform, "HBg", new Color(0, 0, 0, 0.35f));
            UiTheme.SetAnchored(hBg.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-32, 14));
            _healthFill = UiTheme.MakeBarFill(hBg.transform, UiTheme.AccentGreen);

            _energyText = UiTheme.MakeText(vitals.transform, "ENERGY", 12, UiTheme.AccentYellow, FontStyle.Bold);
            UiTheme.SetAnchored(_energyText.rectTransform, new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(16, 28), new Vector2(200, 18));
            var eBg = UiTheme.MakePanel(vitals.transform, "EBg", new Color(0, 0, 0, 0.35f));
            UiTheme.SetAnchored(eBg.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(-32, 14));
            _energyFill = UiTheme.MakeBarFill(eBg.transform, UiTheme.AccentYellow);

            BuildHexAbilities(vitals.transform);

            // Circular virtual joystick
            var stick = UiTheme.MakePanel(root.transform, "Stick", new Color(0.12f, 0.32f, 0.4f, 0.4f), true);
            stick.sprite = ProceduralSprites.Circle("stick-bg", Color.white, 128);
            UiTheme.SetAnchored(stick.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(40, 40), new Vector2(180, 180));
            _stickArea = stick.rectTransform;
            var knob = UiTheme.MakePanel(stick.transform, "Knob", new Color(0.45f, 0.9f, 1f, 0.7f), true);
            knob.sprite = ProceduralSprites.Circle("stick-knob", Color.white, 64);
            UiTheme.SetAnchored(knob.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70, 70));
            _stickKnob = knob.rectTransform;
            var stickHandler = stick.gameObject.AddComponent<StickHandler>();
            stickHandler.Init(this, _stickArea, _stickKnob);

            // Action cluster bottom-right
            var boost = MakeRoundAction(root.transform, "Boost", "BOOST", new Vector2(-210, 150), () => { }, true, 90);
            var chem = MakeRoundAction(root.transform, "Chem", "CHEM", new Vector2(-120, 210), () => GameInput.Instance?.PressTouchChem(), false, 90);
            var evo = MakeRoundAction(root.transform, "Evolve", "DNA", new Vector2(-90, 70), () => ToggleEvolve(), false, 128);
            // Outer DNA ring accent on evolve button
            var dnaRing = UiTheme.MakePanel(evo.transform, "DnaRing", UiTheme.AccentDna * new Color(1, 1, 1, 0.55f));
            dnaRing.sprite = ProceduralSprites.Ring("evo-ring", Color.white, 128, 0.1f);
            dnaRing.raycastTarget = false;
            UiTheme.Stretch(dnaRing);
            dnaRing.transform.SetAsFirstSibling();

            var boostHold = boost.gameObject.AddComponent<HoldButton>();
            boostHold.OnHold = pressed => GameInput.Instance?.SetTouchBoost(pressed);

            _toast = UiTheme.MakeText(root.transform, "", 18, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.SetAnchored(_toast.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(700, 36));

            return root;
        }

        void BuildHexAbilities(Transform parent)
        {
            string[] labels = { "D", "S", "B", "L", "L" };
            Color[] colors = { UiTheme.AccentCyan, UiTheme.AccentGreen, UiTheme.AccentRed, UiTheme.HexSlot, UiTheme.HexSlot };
            for (var i = 0; i < 5; i++)
            {
                var slot = UiTheme.MakePanel(parent, $"Hex{i}", colors[i] * new Color(1, 1, 1, 0.85f));
                UiTheme.SetAnchored(slot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(-80 + i * 40f, 8), new Vector2(34, 34));
                var t = UiTheme.MakeText(slot.transform, i >= 3 ? "-" : labels[i], 12, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
                UiTheme.Stretch(t);
            }
        }

        Button MakeRoundAction(Transform parent, string name, string label, Vector2 anchored, UnityEngine.Events.UnityAction action, bool hold = false, float size = 86)
        {
            var btn = UiTheme.MakeButton(parent, name, new Color(0.08f, 0.25f, 0.32f, 0.85f));
            UiTheme.SetAnchored(btn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), anchored, new Vector2(size, size));
            var img = btn.GetComponent<Image>();
            img.sprite = Visuals.ProceduralSprites.Circle($"btn-{name}", Color.white, 64);
            img.color = new Color(0.15f, 0.4f, 0.5f, 0.8f);
            var text = UiTheme.MakeText(btn.transform, label, 24, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.Stretch(text);
            if (!hold) btn.onClick.AddListener(action);
            return btn;
        }

        GameObject BuildEvolveScreen(Transform parent)
        {
            var root = UiTheme.MakePanel(parent, "EvolveScreen", new Color(0.01f, 0.04f, 0.07f, 0.82f), true).gameObject;
            UiTheme.Stretch(root.transform);

            var card = UiTheme.MakePanel(root.transform, "Card", UiTheme.BgPanelSolid, true);
            UiTheme.SetAnchored(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 620));

            var header = UiTheme.MakeText(card.transform, "EVOLUTION", 28, UiTheme.AccentCyan, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.SetAnchored(header.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(400, 40));

            var grid = new GameObject("Grid");
            grid.transform.SetParent(card.transform, false);
            var gridRt = grid.AddComponent<RectTransform>();
            UiTheme.SetAnchored(gridRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(780, 420));
            var layout = grid.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(240, 84);
            layout.spacing = new Vector2(12, 12);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;

            foreach (var part in PartCatalog.All)
            {
                var local = part;
                var btn = UiTheme.MakeButton(grid.transform, local.Name, UiTheme.HexSlot);
                var label = UiTheme.MakeText(btn.transform, $"{local.Name}\n{local.Cost} DNA — {local.Description}", 13, UiTheme.TextPrimary, FontStyle.Normal, TextAnchor.MiddleCenter);
                UiTheme.Stretch(label);
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                btn.onClick.AddListener(() =>
                {
                    if (PartCatalog.TryBuy(local.Id))
                        AudioDirector.Instance?.PlayUi();
                    RefreshEvolveButtons(grid.transform);
                });
            }

            var close = UiTheme.MakeButton(card.transform, "Close", UiTheme.AccentOrange * new Color(0.5f, 0.5f, 0.5f, 1f));
            UiTheme.SetAnchored(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(200, 48));
            var closeT = UiTheme.MakeText(close.transform, "CLOSE", 18, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.Stretch(closeT);
            close.onClick.AddListener(() => ToggleEvolve(false));

            var customize = UiTheme.MakeButton(card.transform, "ToCustomize", UiTheme.AccentGreen * new Color(0.3f, 0.5f, 0.4f, 1f));
            UiTheme.SetAnchored(customize.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 24), new Vector2(200, 48));
            var ct = UiTheme.MakeText(customize.transform, "CUSTOMIZE", 16, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.Stretch(ct);
            customize.onClick.AddListener(() => { ToggleEvolve(false); OpenCustomize(true); });

            return root;
        }

        void RefreshEvolveButtons(Transform grid)
        {
            // Labels update on next open via rebuild text — simple: tint owned
            var state = GameState.Instance;
            if (state == null) return;
            for (var i = 0; i < grid.childCount && i < PartCatalog.All.Length; i++)
            {
                var part = PartCatalog.All[i];
                var img = grid.GetChild(i).GetComponent<Image>();
                var text = grid.GetChild(i).GetComponentInChildren<Text>();
                var owned = part.IsOwned(state);
                img.color = owned ? UiTheme.AccentGreen * new Color(0.3f, 0.5f, 0.4f, 1f) : UiTheme.HexSlot;
                if (text != null)
                    text.text = owned
                        ? $"{part.Name} ✓\nUnlocked"
                        : $"{part.Name}\n{part.Cost} DNA — {part.Description}";
            }
        }

        GameObject BuildCustomizeScreen(Transform parent)
        {
            var root = UiTheme.MakePanel(parent, "Customize", new Color(0.01f, 0.04f, 0.07f, 0.85f), true).gameObject;
            UiTheme.Stretch(root.transform);
            var card = UiTheme.MakePanel(root.transform, "Card", UiTheme.BgPanelSolid, true);
            UiTheme.SetAnchored(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 520));

            var header = UiTheme.MakeText(card.transform, "CUSTOMIZE CELL", 26, UiTheme.AccentCyan, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.SetAnchored(header.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -18), new Vector2(400, 36));

            var preview = UiTheme.MakePanel(card.transform, "Preview", new Color(0.05f, 0.15f, 0.2f, 0.9f));
            UiTheme.SetAnchored(preview.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(200, 200));
            preview.sprite = ProceduralSprites.SoftEllipse("preview-cell", MembraneSwatches[0], 128, 0.9f, 1.05f, 0.16f, 0.55f);
            preview.preserveAspect = true;
            // Decorative attached-part hints around preview
            string[] partHints = { "Flagella", "Spikes", "Eyes", "Jaws", "Cilia", "Membrane" };
            for (var i = 0; i < partHints.Length; i++)
            {
                var ang = i / (float)partHints.Length * Mathf.PI * 2f - Mathf.PI * 0.5f;
                var chip = UiTheme.MakePanel(card.transform, $"PartChip{i}", UiTheme.HexSlot);
                UiTheme.SetAnchored(chip.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
                    new Vector2(Mathf.Cos(ang) * 175f, Mathf.Sin(ang) * 130f + 30f), new Vector2(88, 28));
                var pt = UiTheme.MakeText(chip.transform, partHints[i], 11, UiTheme.TextDim, FontStyle.Bold, TextAnchor.MiddleCenter);
                UiTheme.Stretch(pt);
            }

            var partsNote = UiTheme.MakeText(card.transform, "Evolved parts attach live on your cell in-world.", 13, UiTheme.TextDim, FontStyle.Normal, TextAnchor.MiddleCenter);
            UiTheme.SetAnchored(partsNote.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 165), new Vector2(500, 24));

            var swatchRow = new GameObject("Swatches");
            swatchRow.transform.SetParent(card.transform, false);
            var srt = swatchRow.AddComponent<RectTransform>();
            UiTheme.SetAnchored(srt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 100), new Vector2(520, 60));
            var hl = swatchRow.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 12;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = false;

            for (var i = 0; i < MembraneSwatches.Length; i++)
            {
                var color = MembraneSwatches[i];
                var btn = UiTheme.MakeButton(swatchRow.transform, $"Swatch{i}", color);
                btn.GetComponent<RectTransform>().sizeDelta = new Vector2(52, 52);
                btn.GetComponent<Image>().sprite = ProceduralSprites.Circle($"swatch-{i}", Color.white, 32);
                btn.GetComponent<Image>().color = color;
                btn.onClick.AddListener(() =>
                {
                    GameState.Instance?.SetMembraneColor(color);
                    _playerLook?.SetMembraneColor(color);
                    _playerLook?.RefreshAttachedParts(GameState.Instance);
                    preview.color = color;
                    preview.sprite = ProceduralSprites.SoftEllipse("preview-cell", color, 128, 0.9f, 1.05f, 0.16f, 0.55f);
                    AudioDirector.Instance?.PlayUi();
                });
            }

            var save = UiTheme.MakeButton(card.transform, "Save", UiTheme.AccentGreen * new Color(0.25f, 0.45f, 0.35f, 1f));
            UiTheme.SetAnchored(save.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(200, 48));
            var st = UiTheme.MakeText(save.transform, "SAVE", 18, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.Stretch(st);
            save.onClick.AddListener(() => OpenCustomize(false));

            return root;
        }

        GameObject BuildOverlay(Transform parent)
        {
            var root = UiTheme.MakePanel(parent, "Overlay", new Color(0, 0, 0, 0.6f), true).gameObject;
            UiTheme.Stretch(root.transform);
            var card = UiTheme.MakePanel(root.transform, "Card", UiTheme.BgPanelSolid, true);
            UiTheme.SetAnchored(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 300));
            _overlayTitle = UiTheme.MakeText(card.transform, "", 28, UiTheme.AccentCyan, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.SetAnchored(_overlayTitle.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(460, 40));
            _overlayBody = UiTheme.MakeText(card.transform, "", 16, UiTheme.TextDim, FontStyle.Normal, TextAnchor.MiddleCenter);
            UiTheme.SetAnchored(_overlayBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(440, 80));

            var resume = UiTheme.MakeButton(card.transform, "Primary", UiTheme.AccentCyan * new Color(0.25f, 0.45f, 0.55f, 1f));
            UiTheme.SetAnchored(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(240, 48));
            var rt = UiTheme.MakeText(resume.transform, "CONTINUE", 18, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.Stretch(rt);
            resume.onClick.AddListener(OnOverlayPrimary);

            var menu = UiTheme.MakeButton(card.transform, "Menu", UiTheme.HexSlot);
            UiTheme.SetAnchored(menu.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(240, 42));
            var mt = UiTheme.MakeText(menu.transform, "MAIN MENU", 16, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            UiTheme.Stretch(mt);
            menu.onClick.AddListener(() => FindObjectOfType<GameBootstrap>()?.ReturnToMenu());
            return root;
        }

        void OnOverlayPrimary()
        {
            var screen = GameFlow.Instance?.Screen;
            if (screen == AppScreen.Paused) GameFlow.Instance.Resume();
            else if (screen == AppScreen.Victory)
            {
                GameState.Instance?.AdvanceGeneration();
                FindObjectOfType<GameBootstrap>()?.BeginRunFromMenu();
            }
            else FindObjectOfType<GameBootstrap>()?.BeginRunFromMenu();
        }

        public void OnStick(Vector2 uiDelta)
        {
            // uiDelta: x right, y up in rect space
            var move = Vector2.ClampMagnitude(uiDelta / 70f, 1f);
            GameInput.Instance?.SetTouchMove(move);
            if (_stickKnob != null)
                _stickKnob.anchoredPosition = Vector2.ClampMagnitude(uiDelta, 55f);
        }

        public void OnStickEnd()
        {
            GameInput.Instance?.SetTouchMove(Vector2.zero);
            if (_stickKnob != null) _stickKnob.anchoredPosition = Vector2.zero;
        }

        void ToggleEvolve(bool? force = null)
        {
            _evolveOpen = force ?? !_evolveOpen;
            _evolveRoot.SetActive(_evolveOpen);
            if (_evolveOpen)
            {
                var grid = _evolveRoot.transform.Find("Card/Grid");
                if (grid != null) RefreshEvolveButtons(grid);
            }
        }

        void OpenCustomize(bool open)
        {
            _customizeOpen = open;
            _customizeRoot.SetActive(open);
            if (open && GameFlow.Instance != null && GameFlow.Instance.Screen == AppScreen.Playing)
                Time.timeScale = 0f;
            else if (!open && GameFlow.Instance != null && GameFlow.Instance.Screen == AppScreen.Playing)
                Time.timeScale = 1f;
        }

        void OnEnable()
        {
            GameEvents.Toast += OnToast;
            if (GameFlow.Instance != null) GameFlow.Instance.ScreenChanged += RefreshScreen;
        }

        void OnDisable()
        {
            GameEvents.Toast -= OnToast;
            if (GameFlow.Instance != null) GameFlow.Instance.ScreenChanged -= RefreshScreen;
        }

        void Start()
        {
            if (GameFlow.Instance != null) GameFlow.Instance.ScreenChanged += RefreshScreen;
            RefreshScreen();
        }

        void OnToast(string msg)
        {
            if (_toast != null) _toast.text = msg;
            _toastTimer = 3f;
        }

        void RefreshScreen()
        {
            var screen = GameFlow.Instance != null ? GameFlow.Instance.Screen : AppScreen.MainMenu;
            _menuRoot.SetActive(screen == AppScreen.MainMenu);
            _playRoot.SetActive(screen == AppScreen.Playing || screen == AppScreen.Paused || screen == AppScreen.Victory || screen == AppScreen.GameOver);
            _overlayRoot.SetActive(screen == AppScreen.Paused || screen == AppScreen.Victory || screen == AppScreen.GameOver);
            if (screen == AppScreen.Paused)
            {
                _overlayTitle.text = "PAUSED";
                _overlayBody.text = "The shoal holds still.";
            }
            else if (screen == AppScreen.Victory)
            {
                _overlayTitle.text = "REPRODUCTION READY";
                _overlayBody.text = "Cell Stage milestone reached.\nPass on your genes to the next generation.";
            }
            else if (screen == AppScreen.GameOver)
            {
                _overlayTitle.text = "COLONY COLLAPSED";
                _overlayBody.text = "Population reached zero.";
            }

            if (screen != AppScreen.Playing)
            {
                _evolveRoot.SetActive(false);
                _evolveOpen = false;
            }
        }

        void Update()
        {
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
                if (_toastTimer <= 0f && _toast != null) _toast.text = "";
            }

            if (GameInput.Instance != null && GameInput.Instance.EvolvePanelPressed)
                ToggleEvolve();
            if (GameInput.Instance != null && GameInput.Instance.PausePressed)
                GameFlow.Instance?.TogglePause();

            // Sync player visual parts
            if (_playerLook != null && GameState.Instance != null)
            {
                _playerLook.SetSpikesVisible(GameState.Instance.HasSpikes);
                _playerLook.SetFlagellaVisible(GameState.Instance.HasFlagella || true);
                _playerLook.SetMembraneColor(GameState.Instance.MembraneColor);
            }

            RefreshPlayStats();
            DrawRadar();
        }

        void RefreshPlayStats()
        {
            var s = GameState.Instance;
            if (s == null || !_playRoot.activeSelf) return;

            _dna.text = $"DNA {s.DNA}";
            _generation.text = $"GENERATION {s.Generation}";
            _score.text = $"SCORE {s.Score}";
            _healthText.text = $"HEALTH  {s.Health:0}/{s.MaxHealth:0}";
            _energyText.text = $"ENERGY  {s.Atp:0}/{s.AtpMax:0}";
            _healthFill.fillAmount = s.Health / Mathf.Max(1f, s.MaxHealth);
            _energyFill.fillAmount = s.Atp / Mathf.Max(1f, s.AtpMax);
            _evoFill.fillAmount = Mathf.Clamp01(s.DNA / 40f);

            var biome = BiomeSystem.Instance != null
                ? BiomeName(BiomeSystem.Instance.ActiveBiome)
                : "Tide Pool";
            _region.text = $"Region: {biome}";
            _title.text = "MicroEvolution";

            var foodDone = s.FoodEaten >= s.FoodObjectiveTarget;
            var bactDone = s.BacteriaEaten >= s.BacteriaObjectiveTarget;
            var flagDone = s.HasFlagella || s.HasOscillator;
            _objectives.text =
                $"{Mark(foodDone)} Absorb {s.FoodObjectiveTarget} Organic Particles  ({Mathf.Min(s.FoodEaten, s.FoodObjectiveTarget)}/{s.FoodObjectiveTarget})\n" +
                $"{Mark(bactDone)} Consume {s.BacteriaObjectiveTarget} Bacteria  ({Mathf.Min(s.BacteriaEaten, s.BacteriaObjectiveTarget)}/{s.BacteriaObjectiveTarget})\n" +
                $"{Mark(flagDone)} Evolve Flagellum / Oscillator\n" +
                $"{Mark(s.PopulationGoalMet)} Colony {s.Population}/{GameConfig.PopulationGoal}\n" +
                $"{Mark(s.BiomeGoalMet)} Reach Thermal Vent";
        }

        void DrawRadar()
        {
            if (_radarTex == null || GameState.Instance == null) return;
            var pixels = new Color[128 * 128];
            for (var y = 0; y < 128; y++)
            for (var x = 0; x < 128; x++)
            {
                var dx = x - 64;
                var dy = y - 64;
                var inside = dx * dx + dy * dy <= 60 * 60;
                pixels[y * 128 + x] = inside
                    ? new Color(0.02f, 0.08f, 0.12f, 0.55f)
                    : new Color(0, 0, 0, 0);
            }

            void Plot(Vector3 world, Color c, int r)
            {
                var nx = 64 + Mathf.RoundToInt(world.x / GameConfig.WorldRadius * 52f);
                var ny = 64 + Mathf.RoundToInt(world.y / GameConfig.WorldRadius * 52f);
                for (var y = -r; y <= r; y++)
                for (var x = -r; x <= r; x++)
                {
                    var px = nx + x;
                    var py = ny + y;
                    if (px < 0 || py < 0 || px >= 128 || py >= 128) continue;
                    var cx = px - 64;
                    var cy = py - 64;
                    if (cx * cx + cy * cy > 60 * 60) continue;
                    if (x * x + y * y <= r * r) pixels[py * 128 + px] = c;
                }
            }

            foreach (var cell in CellRegistry.All)
            {
                if (cell == null) continue;
                Color c;
                switch (cell.Faction)
                {
                    case Faction.Player: c = Color.yellow; break;
                    case Faction.Predator: c = new Color(1f, 0.3f, 0.3f); break;
                    case Faction.Ally: c = new Color(0.3f, 1f, 0.6f); break;
                    default: c = Color.white; break;
                }
                Plot(cell.transform.position, c, cell.IsPlayer ? 3 : 2);
            }

            _radarTex.SetPixels(pixels);
            _radarTex.Apply();
        }

        static string Mark(bool done) => done ? "✓" : "○";

        static string BiomeName(BiomeId id)
        {
            switch (id)
            {
                case BiomeId.TidePool: return "Eukaryotic Shoal";
                case BiomeId.Midwater: return "Midwater Drift";
                default: return "Thermal Vent";
            }
        }

        class StickHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            CinematicHud _hud;
            RectTransform _area;
            RectTransform _knob;
            Canvas _canvas;

            public void Init(CinematicHud hud, RectTransform area, RectTransform knob)
            {
                _hud = hud;
                _area = area;
                _knob = knob;
                _canvas = GetComponentInParent<Canvas>();
            }

            public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

            public void OnDrag(PointerEventData eventData)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, eventData.position, eventData.pressEventCamera, out var local);
                _hud.OnStick(local);
            }

            public void OnPointerUp(PointerEventData eventData) => _hud.OnStickEnd();
        }

        class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
        {
            public System.Action<bool> OnHold;
            public void OnPointerDown(PointerEventData eventData) => OnHold?.Invoke(true);
            public void OnPointerUp(PointerEventData eventData) => OnHold?.Invoke(false);
            public void OnPointerExit(PointerEventData eventData) => OnHold?.Invoke(false);
        }
    }
}
