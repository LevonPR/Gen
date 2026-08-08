using MicroEvolution.Player;
using MicroEvolution.UI;
using MicroEvolution.Visuals;
using MicroEvolution.World;
using UnityEngine;

namespace MicroEvolution.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            // Ensure single bootstrap.
            var existing = FindObjectsOfType<GameBootstrap>();
            if (existing.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            BuildGame();
        }

        void BuildGame()
        {
            Application.targetFrameRate = 60;
            Physics2D.gravity = Vector2.zero;

            // Camera
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

            // Core state
            var stateGo = new GameObject("GameState");
            stateGo.transform.SetParent(transform, false);
            stateGo.AddComponent<GameState>();

            AmbientEnvironment.Create(transform);

            var spawnerGo = new GameObject("WorldSpawner");
            spawnerGo.transform.SetParent(transform, false);
            var spawner = spawnerGo.AddComponent<WorldSpawner>();
            spawner.BuildEcology();

            var player = CreatePlayer(Vector3.zero);
            var follow = camGo.GetComponent<CameraFollow>();
            if (follow == null) follow = camGo.AddComponent<CameraFollow>();
            follow.Configure(player.transform);

            var hudGo = new GameObject("HUD");
            hudGo.transform.SetParent(transform, false);
            var hud = hudGo.AddComponent<GameHUD>();
            hud.Bind(player);

            GameEvents.RaiseToast("Survive, feed, and evolve your microorganism");
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
            if (shader != null)
                trail.material = new Material(shader);
            trail.startColor = new Color(0.4f, 0.85f, 1f, 0.45f);
            trail.endColor = new Color(0.4f, 0.85f, 1f, 0f);
            trail.sortingOrder = 1;

            var player = go.AddComponent<PlayerController>();
            player.Init(pos);
            return player;
        }
    }
}
