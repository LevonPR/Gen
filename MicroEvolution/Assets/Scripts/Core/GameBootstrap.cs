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
        GameHUD _hud;
        bool _runAlive;

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

            var hudGo = new GameObject("HUD");
            hudGo.transform.SetParent(transform, false);
            _hud = hudGo.AddComponent<GameHUD>();
            var touch = hudGo.AddComponent<TouchControls>();
            touch.Configure();
            var tutorial = hudGo.AddComponent<TutorialDirector>();
            _hud.Bind(null, touch, tutorial);

            AmbientEnvironment.Create(transform);
            var biome = gameObject.AddComponent<BiomeSystem>();
            biome.BuildVeil(transform);

            GameFlow.Instance.ShowMainMenu();
            GameEvents.RaiseToast("MicroEvolution — Cell Stage");
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
            GameFlow.Instance.ShowMainMenu();
        }

        void StartRun()
        {
            var stateGo = new GameObject("GameState");
            stateGo.transform.SetParent(transform, false);
            stateGo.AddComponent<GameState>();

            _worldRoot = new GameObject("RunWorld").transform;
            _worldRoot.SetParent(transform, false);

            var spawnerGo = new GameObject("WorldSpawner");
            spawnerGo.transform.SetParent(_worldRoot, false);
            spawnerGo.AddComponent<WorldSpawner>().BuildEcology();

            _player = CreatePlayer(Vector3.zero);
            var cam = Camera.main;
            if (cam != null)
            {
                var follow = cam.GetComponent<CameraFollow>();
                if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
                follow.Configure(_player.transform);
                if (cam.GetComponent<CameraImpulse>() == null)
                    cam.gameObject.AddComponent<CameraImpulse>();
                cam.orthographicSize = MobileSettings.IsMobileRuntime
                    ? GameConfig.CameraSizeMobile
                    : GameConfig.CameraSize;
            }

            var touch = _hud.GetComponent<TouchControls>();
            var tutorial = _hud.GetComponent<TutorialDirector>();
            _hud.Bind(_player, touch, tutorial);
            _runAlive = true;
            SaveSystem.SaveMeta(GameFlow.Instance, GameState.Instance);
        }

        void TeardownRun()
        {
            if (_worldRoot != null) Destroy(_worldRoot.gameObject);
            if (_player != null) Destroy(_player.gameObject);
            var state = GameState.Instance;
            if (state != null) Destroy(state.gameObject);
            _player = null;
            _runAlive = false;
            _hud?.Bind(null, _hud.GetComponent<TouchControls>(), _hud.GetComponent<TutorialDirector>());
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
            appearance.Build(Faction.Player, GameConfig.PlayerRadius, new Color(0.35f, 0.8f, 1f, 0.72f));

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.45f;
            trail.startWidth = 0.35f;
            trail.endWidth = 0.02f;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null) trail.material = new Material(shader);
            trail.startColor = new Color(0.4f, 0.85f, 1f, 0.45f);
            trail.endColor = new Color(0.4f, 0.85f, 1f, 0f);
            trail.sortingOrder = 1;

            var player = go.AddComponent<PlayerController>();
            player.Init(pos);
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
            cam.backgroundColor = new Color(0.02f, 0.07f, 0.12f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.depth = -1;
        }
    }
}
