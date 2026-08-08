using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.World
{
    public enum BiomeId
    {
        TidePool = 0,
        Midwater = 1,
        ThermalVent = 2
    }

    public class BiomeSystem : MonoBehaviour
    {
        public static BiomeSystem Instance { get; private set; }

        public BiomeId ActiveBiome { get; private set; } = BiomeId.TidePool;
        public float Darkness { get; private set; }

        SpriteRenderer _veil;

        void Awake()
        {
            Instance = this;
        }

        public void BuildVeil(Transform parent)
        {
            var go = new GameObject("BiomeVeil");
            go.transform.SetParent(parent, false);
            _veil = go.AddComponent<SpriteRenderer>();
            _veil.sprite = Visuals.ProceduralSprites.Circle("veil", new Color(0.02f, 0.04f, 0.08f, 0.55f), 64);
            _veil.sortingOrder = 40;
            go.transform.localScale = Vector3.one * 80f;
            _veil.enabled = false;
        }

        void Update()
        {
            if (GameState.Instance == null || GameState.Instance.PlayerTransform == null) return;

            var dist = GameState.Instance.PlayerTransform.position.magnitude;
            BiomeId biome;
            if (dist < GameConfig.TidePoolRadius) biome = BiomeId.TidePool;
            else if (dist < GameConfig.MidwaterRadius) biome = BiomeId.Midwater;
            else biome = BiomeId.ThermalVent;

            if (biome != ActiveBiome)
                ActiveBiome = biome;

            GameState.Instance.SetBiomeIndex((int)biome);

            // Darkness in vent without eyes
            var wantDark = biome == BiomeId.ThermalVent && !GameState.Instance.HasEyes;
            Darkness = Mathf.MoveTowards(Darkness, wantDark ? 1f : 0f, Time.deltaTime * 1.5f);
            if (_veil != null)
            {
                _veil.enabled = Darkness > 0.01f;
                var c = _veil.color;
                c.a = 0.15f + Darkness * 0.55f;
                _veil.color = c;
                if (GameState.Instance.PlayerTransform != null)
                    _veil.transform.position = GameState.Instance.PlayerTransform.position;
            }

            // Mild toxin pressure in vent
            if (biome == BiomeId.ThermalVent && !GameState.Instance.PlayerDead && !GameState.Instance.HasMembrane)
            {
                if (Time.frameCount % 30 == 0)
                    GameState.Instance.Damage(1.2f);
            }
        }

        public static string BiomeName(BiomeId id)
        {
            switch (id)
            {
                case BiomeId.TidePool: return "Biome: Tide Pool";
                case BiomeId.Midwater: return "Biome: Midwater";
                default: return "Biome: Thermal Vent";
            }
        }

        public static Color FoodTint(BiomeId id)
        {
            switch (id)
            {
                case BiomeId.TidePool: return new Color(0.45f, 0.95f, 0.55f, 0.9f);
                case BiomeId.Midwater: return new Color(0.55f, 0.85f, 1f, 0.9f);
                default: return new Color(1f, 0.55f, 0.25f, 0.9f);
            }
        }
    }
}
