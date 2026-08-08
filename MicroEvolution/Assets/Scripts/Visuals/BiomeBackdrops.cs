using MicroEvolution.Core;
using MicroEvolution.World;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Layered procedural backdrops that shift with biome — stands in for painted environment art.
    /// </summary>
    public class BiomeBackdrops : MonoBehaviour
    {
        SpriteRenderer[] _layers;
        Vector3[] _basePos;
        BiomeId _last = (BiomeId)(-1);

        public static BiomeBackdrops Create(Transform parent)
        {
            var go = new GameObject("BiomeBackdrops");
            go.transform.SetParent(parent, false);
            var b = go.AddComponent<BiomeBackdrops>();
            b.Build();
            return b;
        }

        void Build()
        {
            _layers = new SpriteRenderer[5];
            _basePos = new Vector3[5];
            for (var i = 0; i < _layers.Length; i++)
            {
                var layer = new GameObject($"Backdrop{i}");
                layer.transform.SetParent(transform, false);
                var sr = layer.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -38 + i;
                layer.transform.localScale = Vector3.one * (GameConfig.WorldRadius * (1.3f + i * 0.25f));
                _basePos[i] = (Vector3)(Random.insideUnitCircle * (6f + i * 3f));
                layer.transform.position = _basePos[i];
                _layers[i] = sr;
            }

            Apply(BiomeId.TidePool);
        }

        void Update()
        {
            if (BiomeSystem.Instance == null) return;
            if (BiomeSystem.Instance.ActiveBiome != _last)
            {
                _last = BiomeSystem.Instance.ActiveBiome;
                Apply(_last);
            }

            if (GameState.Instance != null && GameState.Instance.PlayerTransform != null)
            {
                var p = GameState.Instance.PlayerTransform.position;
                for (var i = 0; i < _layers.Length; i++)
                {
                    var parallax = 0.03f + i * 0.02f;
                    _layers[i].transform.position = _basePos[i] + new Vector3(p.x * parallax, p.y * parallax, 0f);
                }
            }
        }

        void Apply(BiomeId biome)
        {
            Color[] colors;
            switch (biome)
            {
                case BiomeId.TidePool:
                    colors = new[]
                    {
                        new Color(0.05f, 0.22f, 0.18f, 0.35f),
                        new Color(0.08f, 0.28f, 0.22f, 0.28f),
                        new Color(0.12f, 0.2f, 0.16f, 0.22f),
                        new Color(0.15f, 0.12f, 0.1f, 0.2f),
                        new Color(0.05f, 0.15f, 0.2f, 0.18f),
                    };
                    break;
                case BiomeId.Midwater:
                    colors = new[]
                    {
                        new Color(0.04f, 0.1f, 0.28f, 0.4f),
                        new Color(0.06f, 0.14f, 0.35f, 0.3f),
                        new Color(0.08f, 0.18f, 0.4f, 0.22f),
                        new Color(0.1f, 0.08f, 0.25f, 0.2f),
                        new Color(0.03f, 0.08f, 0.2f, 0.18f),
                    };
                    break;
                default:
                    colors = new[]
                    {
                        new Color(0.28f, 0.08f, 0.04f, 0.4f),
                        new Color(0.35f, 0.12f, 0.05f, 0.3f),
                        new Color(0.2f, 0.05f, 0.04f, 0.25f),
                        new Color(0.4f, 0.15f, 0.05f, 0.2f),
                        new Color(0.15f, 0.04f, 0.03f, 0.22f),
                    };
                    break;
            }

            for (var i = 0; i < _layers.Length; i++)
            {
                _layers[i].sprite = ProceduralSprites.SoftEllipse(
                    $"backdrop-{biome}-{i}", colors[i], 128,
                    1f + i * 0.1f, 0.8f + i * 0.05f, 0.25f, 0.15f);
                _layers[i].color = Color.white;
            }
        }
    }
}
