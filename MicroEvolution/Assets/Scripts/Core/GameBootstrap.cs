using MicroEvolution.Audio;
using MicroEvolution.InputSystem;
using MicroEvolution.Mobile;
using MicroEvolution.Player;
using MicroEvolution.UI;
using MicroEvolution.Visuals;
using MicroEvolution.World;
using UnityEngine;

namespace MicroEvolution.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        Transform _worldRoot;
        PlayerController _player;
        CinematicHud _hud;

        void Awake()
        {
            var existing = FindObjectsOfType<GameBootstrap>();
            if (existing.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            BuildShell();
        }

        void BuildShell()
        {
            MobileSettings.ApplyRuntimeFlags();
            Physics2D.gravity = Vector2.zero;

            EnsureCamera();
            gameObject.AddComponent<GameFlow>().LoadMeta();
            gameObject.AddComponent<GameInput>();
            gameObject.AddComponent<AudioDirector>();
            gameObject.AddComponent<ObjectPool>();

            UnderwaterAtmosphere.Create(transform);
            VolumetricRays.Create(transform);
            BiomeBackdrops.Create(transform);
            CausticsOverlay.Create(transform);
            var biome = gameObject.AddComponent<BiomeSystem>();
            biome.BuildVeil(transform);
            gameObject.AddComponent<BiomeAtmosphere>();

            var hudGo = new GameObject("CinematicHUD");
            hudGo.transform.SetParent(transform, false);
            _hud = hudGo.AddComponent<CinematicHud>();
            _hud.Build();

            GameFlow.Instance.ShowMainMenu();
            GameEvents.RaiseToast("Swim. Feed. Evolve.");
        }

        public void BeginRunFromMenu()
        {
            TeardownRun();
            StartRun();
            GameFlow.Instance.StartNewRun();
        }

        public void ReturnToMenu()
        {
            TeardownRun();
            Time.timeScale = 1f;
            GameFlow.Instance.ShowMainMenu();
        }

        void StartRun()
        {
            var stateGo = new GameObject("GameState");
            stateGo.transform.SetParent(transform, false);
            stateGo.AddComponent<GameState>();

            _worldRoot = new GameObject("RunWorld").transform;
            _worldRoot.SetParent(transform, false);

            ArtModelLibrary.WarmupAsync(
                "player_core", "prey_rod", "predator_spiky", "predator_worm", "ally_probe", "food_pellet");

            var spawnerGo = new GameObject("WorldSpawner");
            spawnerGo.transform.SetParent(_worldRoot, false);
            spawnerGo.AddComponent<WorldSpawner>().BuildEcology();

            _player = CreatePlayer(Vector3.zero);
            var look = _player.GetComponent<CellAppearance>();
            look?.SetMembraneColor(GameState.Instance.MembraneColor);

            var cam = Camera.main;
            if (cam != null)
            {
                var follow = cam.GetComponent<CameraFollow>();
                if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
                follow.Configure(_player.transform);
                if (cam.GetComponent<CameraImpulse>() == null)
                    cam.gameObject.AddComponent<CameraImpulse>();
                if (cam.GetComponent<CameraEffects>() == null)
                    cam.gameObject.AddComponent<CameraEffects>();
                if (cam.GetComponent<CinematicPostProcess>() == null)
                    cam.gameObject.AddComponent<CinematicPostProcess>();
                BiomeAtmosphere.Instance?.BindCamera(cam);
                cam.orthographicSize = MobileSettings.IsMobileRuntime
                    ? GameConfig.CameraSizeMobile
                    : GameConfig.CameraSize;
                cam.backgroundColor = new Color(0.015f, 0.06f, 0.1f, 1f);
            }

            _hud.BindPlayer(_player);
            SaveSystem.SaveMeta(GameFlow.Instance, GameState.Instance);
        }

        void TeardownRun()
        {
            if (_worldRoot != null) Destroy(_worldRoot.gameObject);
            if (_player != null) Destroy(_player.gameObject);
            var state = GameState.Instance;
            if (state != null) Destroy(state.gameObject);
            _player = null;
            _hud?.BindPlayer(null);
        }

        PlayerController CreatePlayer(Vector3 pos)
        {
            var go = new GameObject("PlayerCell");
            go.transform.position = pos;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.drag = 2.8f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = GameConfig.PlayerRadius;

            go.AddComponent<CellMotor>();
            go.AddComponent<LivingCell>();
            var appearance = go.AddComponent<CellAppearance>();
            var color = GameState.Instance != null
                ? GameState.Instance.MembraneColor
                : new Color(0.35f, 0.8f, 1f, 0.72f);
            appearance.Build(Faction.Player, GameConfig.PlayerRadius, color);

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.55f;
            trail.startWidth = 0.4f;
            trail.endWidth = 0.02f;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null) trail.material = new Material(shader);
            trail.startColor = new Color(color.r, color.g, color.b, 0.5f);
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
            trail.sortingOrder = 1;

            var player = go.AddComponent<PlayerController>();
            player.Init(pos);
            BiolumLight.Attach(go.transform, color);
            return player;
        }

        void EnsureCamera()
        {
            var camGo = GameObject.Find("Main Camera");
            Camera cam;
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }
            else
            {
                cam = camGo.GetComponent<Camera>();
                if (cam == null) cam = camGo.AddComponent<Camera>();
                if (camGo.GetComponent<AudioListener>() == null)
                    camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = GameConfig.CameraSize;
            cam.backgroundColor = new Color(0.015f, 0.06f, 0.1f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.depth = -1;
        }
    }
}
