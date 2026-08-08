using MicroEvolution.Core;
using MicroEvolution.Visuals;
using UnityEngine;

namespace MicroEvolution.World
{
    public class FoodPellet : MonoBehaviour
    {
        float _spin;

        public static FoodPellet Spawn(Transform parent, Vector2 position)
        {
            var go = new GameObject("Food");
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            var tint = Color.Lerp(new Color(0.45f, 0.95f, 0.55f, 0.9f), new Color(0.85f, 0.95f, 0.4f, 0.9f), Random.value);
            sr.sprite = ProceduralSprites.Circle("food", tint, 32);
            sr.sortingOrder = 2;
            go.transform.localScale = Vector3.one * Random.Range(0.25f, 0.4f);

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.drag = 1f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            return go.AddComponent<FoodPellet>();
        }

        void Update()
        {
            _spin += Time.deltaTime * 40f;
            transform.rotation = Quaternion.Euler(0f, 0f, _spin);
            transform.position += (Vector3)(Random.insideUnitCircle * (0.15f * Time.deltaTime));
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var cell = other.GetComponentInParent<LivingCell>();
            if (cell == null) return;
            if (cell.Faction != Faction.Player && cell.Faction != Faction.Ally && cell.Faction != Faction.Prey)
                return;

            if (cell.IsPlayer && GameState.Instance != null)
            {
                GameState.Instance.AddBiomass(GameConfig.FoodBiomassValue);
                GameState.Instance.AddAtp(8f);
                if (Random.value < 0.25f)
                    GameState.Instance.AddEvolutionPoints(GameConfig.FoodEvoReward);
            }

            // Prey/allies "eat" by destroying food; respawn handled by WorldSpawner.
            Destroy(gameObject);
        }
    }
}
