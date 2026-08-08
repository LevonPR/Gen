using MicroEvolution.Core;
using MicroEvolution.Visuals;
using UnityEngine;

namespace MicroEvolution.World
{
    public class FoodPellet : MonoBehaviour
    {
        float _spin;
        SpriteRenderer _sr;
        SpriteRenderer _glow;

        public static FoodPellet Spawn(Transform parent, Vector2 position)
        {
            GameObject go;
            if (ObjectPool.Instance != null)
                go = ObjectPool.Instance.Get("food", CreateNew);
            else
                go = CreateNew();

            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var pellet = go.GetComponent<FoodPellet>();
            pellet.Restyle();
            return pellet;
        }

        static GameObject CreateNew()
        {
            var go = new GameObject("Food");
            var glow = new GameObject("Glow");
            glow.transform.SetParent(go.transform, false);
            var gsr = glow.AddComponent<SpriteRenderer>();
            gsr.sprite = ProceduralSprites.BloomDisc("food-bloom", new Color(0.5f, 1f, 0.6f, 0.45f), 64);
            gsr.sortingOrder = 1;
            glow.transform.localScale = Vector3.one * 1.8f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.SoftEllipse("food-core", Color.white, 48, 1f, 1f, 0.2f, 0.6f);
            sr.sortingOrder = 2;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.drag = 1f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            var pellet = go.AddComponent<FoodPellet>();
            pellet._glow = gsr;
            return go;
        }

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_glow == null)
            {
                var g = transform.Find("Glow");
                if (g != null) _glow = g.GetComponent<SpriteRenderer>();
            }
        }

        void Restyle()
        {
            var biome = BiomeId.TidePool;
            var dist = transform.position.magnitude;
            if (dist >= GameConfig.MidwaterRadius) biome = BiomeId.ThermalVent;
            else if (dist >= GameConfig.TidePoolRadius) biome = BiomeId.Midwater;

            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            var tint = BiomeSystem.FoodTint(biome);
            _sr.color = tint;
            if (_glow != null) _glow.color = new Color(tint.r, tint.g, tint.b, 0.4f);
            transform.localScale = Vector3.one * Random.Range(0.28f, 0.45f);
        }

        void Update()
        {
            _spin += Time.deltaTime * 40f;
            transform.rotation = Quaternion.Euler(0f, 0f, _spin);
            var pulse = 1f + Mathf.Sin(Time.time * 3f + transform.position.x) * 0.08f;
            if (_glow != null) _glow.transform.localScale = Vector3.one * (1.8f * pulse);
            transform.position += (Vector3)(Random.insideUnitCircle * (0.12f * Time.deltaTime));
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var cell = other.GetComponentInParent<LivingCell>();
            if (cell == null) return;
            if (cell.Faction != Faction.Player && cell.Faction != Faction.Ally && cell.Faction != Faction.Prey)
                return;

            if (cell.IsPlayer && GameState.Instance != null)
            {
                var value = GameConfig.FoodBiomassValue;
                if (BiomeSystem.Instance != null && BiomeSystem.Instance.ActiveBiome == BiomeId.ThermalVent)
                    value *= 1.35f;

                GameState.Instance.AddBiomass(value, countsAsOrganicParticle: true);
                GameState.Instance.AddAtp(8f);
                if (Random.value < 0.28f)
                    GameState.Instance.AddEvolutionPoints(GameConfig.FoodEvoReward);
                GameEvents.RaiseAteFood();
                VfxBurst.Spawn(transform.position, new Color(0.5f, 1f, 0.5f, 0.85f), 0.7f);
                FloatingText.Spawn(transform.position, $"+{value:0}", new Color(0.6f, 1f, 0.6f));
            }

            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Release("food", gameObject);
            else
                Destroy(gameObject);
        }
    }
}
