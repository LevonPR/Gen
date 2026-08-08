using MicroEvolution.Core;
using MicroEvolution.Visuals;
using UnityEngine;

namespace MicroEvolution.World
{
    public class FoodPellet : MonoBehaviour
    {
        float _spin;
        SpriteRenderer _sr;

        public static FoodPellet Spawn(Transform parent, Vector2 position)
        {
            GameObject go;
            if (ObjectPool.Instance != null)
            {
                go = ObjectPool.Instance.Get("food", () => CreateNew());
            }
            else
            {
                go = CreateNew();
            }

            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var pellet = go.GetComponent<FoodPellet>();
            pellet.Restyle();
            return pellet;
        }

        static GameObject CreateNew()
        {
            var go = new GameObject("Food");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.Circle("food", Color.white, 32);
            sr.sortingOrder = 2;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.drag = 1f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<FoodPellet>();
            return go;
        }

        void Awake() => _sr = GetComponent<SpriteRenderer>();

        void Restyle()
        {
            var biome = BiomeId.TidePool;
            var dist = transform.position.magnitude;
            if (dist >= GameConfig.MidwaterRadius) biome = BiomeId.ThermalVent;
            else if (dist >= GameConfig.TidePoolRadius) biome = BiomeId.Midwater;

            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            _sr.color = BiomeSystem.FoodTint(biome);
            transform.localScale = Vector3.one * Random.Range(0.25f, 0.4f);
        }

        void Update()
        {
            _spin += Time.deltaTime * 40f;
            transform.rotation = Quaternion.Euler(0f, 0f, _spin);
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

                GameState.Instance.AddBiomass(value);
                GameState.Instance.AddAtp(8f);
                if (Random.value < 0.28f)
                    GameState.Instance.AddEvolutionPoints(GameConfig.FoodEvoReward);
                GameEvents.RaiseAteFood();
                VfxBurst.Spawn(transform.position, new Color(0.5f, 1f, 0.5f, 0.8f), 0.5f);
                FloatingText.Spawn(transform.position, $"+{value:0}", new Color(0.6f, 1f, 0.6f));
            }

            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Release("food", gameObject);
            else
                Destroy(gameObject);
        }
    }
}
