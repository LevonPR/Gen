using MicroEvolution.Core;
using MicroEvolution.Visuals;
using UnityEngine;

namespace MicroEvolution.World
{
    /// <summary>
    /// Drives post-process grade + particle tint as the player crosses biomes.
    /// </summary>
    public class BiomeAtmosphere : MonoBehaviour
    {
        public static BiomeAtmosphere Instance { get; private set; }

        CinematicPostProcess _post;
        BiomeId _last = (BiomeId)(-1);
        SpriteRenderer _haze;

        void Awake()
        {
            Instance = this;
            var hazeGo = new GameObject("BiomeHaze");
            hazeGo.transform.SetParent(transform, false);
            _haze = hazeGo.AddComponent<SpriteRenderer>();
            _haze.sprite = ProceduralSprites.BloomDisc("biome-haze", Color.white, 128);
            _haze.sortingOrder = 35;
            hazeGo.transform.localScale = Vector3.one * 40f;
            _haze.color = new Color(0.2f, 0.5f, 0.6f, 0f);
        }

        public void BindCamera(Camera cam)
        {
            if (cam == null) return;
            _post = cam.GetComponent<CinematicPostProcess>();
            if (_post == null) _post = cam.gameObject.AddComponent<CinematicPostProcess>();
        }

        void Update()
        {
            if (BiomeSystem.Instance == null) return;
            var biome = BiomeSystem.Instance.ActiveBiome;
            if (biome != _last)
            {
                var first = (int)_last < 0;
                _last = biome;
                Apply(biome);
                if (!first) GameEvents.RaiseToast(BiomeSystem.BiomeName(biome));
            }

            if (_haze != null && GameState.Instance != null && GameState.Instance.PlayerTransform != null)
                _haze.transform.position = GameState.Instance.PlayerTransform.position;
        }

        void Apply(BiomeId biome)
        {
            Color tint;
            float bloom;
            float distort;
            Color haze;
            switch (biome)
            {
                case BiomeId.TidePool:
                    tint = new Color(0.9f, 1.02f, 1.05f, 1f);
                    bloom = 0.85f;
                    distort = 0.002f;
                    haze = new Color(0.25f, 0.7f, 0.55f, 0.05f);
                    break;
                case BiomeId.Midwater:
                    tint = new Color(0.82f, 0.94f, 1.12f, 1f);
                    bloom = 0.95f;
                    distort = 0.004f;
                    haze = new Color(0.2f, 0.45f, 0.85f, 0.08f);
                    break;
                default: // ThermalVent
                    tint = new Color(1.12f, 0.88f, 0.78f, 1f);
                    bloom = 1.15f;
                    distort = 0.012f;
                    haze = new Color(1f, 0.35f, 0.15f, 0.12f);
                    break;
            }

            _post?.SetBiomeGrade(tint, bloom, distort);
            if (_haze != null) _haze.color = haze;
            if (Camera.main != null)
            {
                switch (biome)
                {
                    case BiomeId.TidePool:
                        Camera.main.backgroundColor = new Color(0.02f, 0.09f, 0.1f);
                        break;
                    case BiomeId.Midwater:
                        Camera.main.backgroundColor = new Color(0.01f, 0.05f, 0.12f);
                        break;
                    default:
                        Camera.main.backgroundColor = new Color(0.08f, 0.03f, 0.02f);
                        break;
                }
            }
        }
    }
}
