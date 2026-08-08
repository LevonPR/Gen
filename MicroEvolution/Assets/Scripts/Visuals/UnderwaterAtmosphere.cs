using MicroEvolution.Core;
using MicroEvolution.Mobile;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Cinematic microscopic water: god rays, marine snow, soft depth discs, vignette feel.
    /// </summary>
    public class UnderwaterAtmosphere : MonoBehaviour
    {
        Transform _rayRoot;
        Transform _snowRoot;
        float _pulse;

        public static UnderwaterAtmosphere Create(Transform parent)
        {
            var go = new GameObject("UnderwaterAtmosphere");
            go.transform.SetParent(parent, false);
            var fx = go.AddComponent<UnderwaterAtmosphere>();
            fx.Build();
            return fx;
        }

        void Build()
        {
            // Deep backdrop discs (fake depth layers)
            for (var i = 0; i < 4; i++)
            {
                var layer = new GameObject($"DepthLayer{i}");
                layer.transform.SetParent(transform, false);
                var sr = layer.AddComponent<SpriteRenderer>();
                var a = 0.18f - i * 0.03f;
                sr.sprite = ProceduralSprites.Circle($"depth-{i}", new Color(0.02f + i * 0.01f, 0.1f + i * 0.02f, 0.14f + i * 0.02f, a), 128);
                sr.sortingOrder = -40 + i;
                layer.transform.localScale = Vector3.one * (GameConfig.WorldRadius * (1.6f + i * 0.35f));
                layer.transform.position = Random.insideUnitCircle * 8f;
            }

            // Organic terrain blobs (blurred background life)
            for (var i = 0; i < 14; i++)
            {
                var blob = new GameObject($"Terrain{i}");
                blob.transform.SetParent(transform, false);
                blob.transform.position = (Vector3)(Random.insideUnitCircle * GameConfig.WorldRadius * 0.85f);
                var sr = blob.AddComponent<SpriteRenderer>();
                var warm = Random.value > 0.5f;
                sr.sprite = ProceduralSprites.Circle($"terrain-{i % 3}",
                    warm ? new Color(0.15f, 0.08f, 0.12f, 0.4f) : new Color(0.05f, 0.18f, 0.16f, 0.45f), 96);
                sr.sortingOrder = -30;
                blob.transform.localScale = Vector3.one * Random.Range(3.5f, 9f);
            }

            // God rays
            _rayRoot = new GameObject("GodRays").transform;
            _rayRoot.SetParent(transform, false);
            for (var i = 0; i < 7; i++)
            {
                var ray = new GameObject($"Ray{i}");
                ray.transform.SetParent(_rayRoot, false);
                var sr = ray.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Circle("ray", new Color(0.45f, 0.85f, 1f, 0.08f), 64);
                sr.sortingOrder = -25;
                ray.transform.localScale = new Vector3(Random.Range(1.2f, 2.8f), Random.Range(18f, 32f), 1f);
                ray.transform.position = new Vector3(Random.Range(-40f, 40f), Random.Range(5f, 25f), 0f);
                ray.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));
            }

            // World rim
            var rim = new GameObject("WorldRim");
            rim.transform.SetParent(transform, false);
            var rimSr = rim.AddComponent<SpriteRenderer>();
            rimSr.sprite = ProceduralSprites.Ring("world-rim", new Color(0.2f, 0.7f, 0.85f, 0.28f), 128, 0.06f);
            rimSr.sortingOrder = -18;
            rim.transform.localScale = Vector3.one * (GameConfig.WorldRadius * 2.05f);

            // Marine snow
            _snowRoot = new GameObject("MarineSnow").transform;
            _snowRoot.SetParent(transform, false);
            var count = MobileSettings.IsMobileRuntime ? 70 : 140;
            for (var i = 0; i < count; i++)
            {
                var p = new GameObject($"Snow{i}");
                p.transform.SetParent(_snowRoot, false);
                p.transform.position = (Vector3)(Random.insideUnitCircle * GameConfig.WorldRadius);
                var sr = p.AddComponent<SpriteRenderer>();
                var bright = Random.Range(0.12f, 0.55f);
                sr.sprite = ProceduralSprites.Circle("snow", new Color(0.75f, 0.95f, 1f, bright), 16);
                sr.sortingOrder = -8;
                p.transform.localScale = Vector3.one * Random.Range(0.04f, 0.2f);
                var drift = p.AddComponent<SnowDrift>();
                drift.Speed = Random.Range(0.04f, 0.28f);
                drift.Sway = Random.Range(0.2f, 1.2f);
            }

            // Soft bioluminescent motes
            for (var i = 0; i < 20; i++)
            {
                var mote = new GameObject($"Mote{i}");
                mote.transform.SetParent(transform, false);
                mote.transform.position = (Vector3)(Random.insideUnitCircle * GameConfig.WorldRadius * 0.9f);
                var sr = mote.AddComponent<SpriteRenderer>();
                var c = Color.Lerp(new Color(0.3f, 1f, 0.7f, 0.35f), new Color(0.4f, 0.7f, 1f, 0.35f), Random.value);
                sr.sprite = ProceduralSprites.Circle("mote", c, 32);
                sr.sortingOrder = -6;
                mote.transform.localScale = Vector3.one * Random.Range(0.35f, 1.1f);
            }
        }

        void Update()
        {
            _pulse += Time.deltaTime * 0.15f;
            if (_rayRoot != null)
                _rayRoot.localPosition = new Vector3(Mathf.Sin(_pulse) * 2f, Mathf.Cos(_pulse * 0.7f) * 1.2f, 0f);
        }

        class SnowDrift : MonoBehaviour
        {
            public float Speed = 0.1f;
            public float Sway = 0.5f;
            float _t;
            Vector2 _dir;

            void Start()
            {
                _dir = Random.insideUnitCircle.normalized;
                _t = Random.value * 10f;
            }

            void Update()
            {
                _t += Time.deltaTime;
                var sway = new Vector2(Mathf.Sin(_t * Sway), Mathf.Cos(_t * Sway * 0.7f)) * 0.15f;
                transform.position += (Vector3)((_dir + sway) * Speed * Time.deltaTime);
                if (transform.position.magnitude > GameConfig.WorldRadius)
                {
                    transform.position = (Vector3)(-transform.position.normalized * GameConfig.WorldRadius * 0.92f);
                    _dir = Random.insideUnitCircle.normalized;
                }
            }
        }
    }
}
