using MicroEvolution.Core;
using MicroEvolution.Visuals;
using UnityEngine;

namespace MicroEvolution.World
{
    public class AmbientEnvironment : MonoBehaviour
    {
        public static void Create(Transform parent)
        {
            var root = new GameObject("Ambient");
            root.transform.SetParent(parent, false);
            root.AddComponent<AmbientEnvironment>();

            // Soft world disc / floor hint
            var floor = new GameObject("FloorGlow");
            floor.transform.SetParent(root.transform, false);
            var floorSr = floor.AddComponent<SpriteRenderer>();
            floorSr.sprite = ProceduralSprites.Circle("floor", new Color(0.05f, 0.18f, 0.28f, 0.55f), 128);
            floorSr.sortingOrder = -20;
            floor.transform.localScale = Vector3.one * (GameConfig.WorldRadius * 2.15f);

            var rim = new GameObject("WorldRim");
            rim.transform.SetParent(root.transform, false);
            var rimSr = rim.AddComponent<SpriteRenderer>();
            rimSr.sprite = ProceduralSprites.Ring("rim", new Color(0.2f, 0.55f, 0.75f, 0.35f), 128, 0.08f);
            rimSr.sortingOrder = -19;
            rim.transform.localScale = Vector3.one * (GameConfig.WorldRadius * 2.05f);

            // Marine snow
            for (var i = 0; i < 120; i++)
            {
                var p = new GameObject($"Snow{i}");
                p.transform.SetParent(root.transform, false);
                p.transform.position = (Vector3)(Random.insideUnitCircle * GameConfig.WorldRadius);
                var sr = p.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Circle("snow", new Color(0.85f, 0.95f, 1f, Random.Range(0.15f, 0.45f)), 16);
                sr.sortingOrder = -5;
                var scale = Random.Range(0.05f, 0.18f);
                p.transform.localScale = Vector3.one * scale;
                var drift = p.AddComponent<DriftMotion>();
                drift.Speed = Random.Range(0.05f, 0.25f);
            }

            // Moss patches
            for (var i = 0; i < 18; i++)
            {
                var moss = new GameObject($"Moss{i}");
                moss.transform.SetParent(root.transform, false);
                moss.transform.position = (Vector3)WorldSpawner.RandomPoint(2f);
                var sr = moss.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Circle("moss", new Color(0.15f, 0.35f, 0.28f, 0.55f), 64);
                sr.sortingOrder = -10;
                moss.transform.localScale = Vector3.one * Random.Range(1.5f, 3.5f);
            }
        }

        class DriftMotion : MonoBehaviour
        {
            public float Speed = 0.1f;
            Vector2 _dir;

            void Start() => _dir = Random.insideUnitCircle.normalized;

            void Update()
            {
                transform.position += (Vector3)(_dir * Speed * Time.deltaTime);
                if (transform.position.magnitude > GameConfig.WorldRadius)
                {
                    transform.position = (Vector3)(-transform.position.normalized * GameConfig.WorldRadius * 0.9f);
                    _dir = Random.insideUnitCircle.normalized;
                }
            }
        }
    }
}
