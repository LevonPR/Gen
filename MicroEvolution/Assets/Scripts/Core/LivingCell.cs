using System;
using UnityEngine;

namespace MicroEvolution.Core
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class LivingCell : MonoBehaviour
    {
        public Faction Faction;
        public float MaxHealth = 50f;
        public float Health;
        public float ContactDamage = 12f;
        public float Radius = 0.7f;
        public float BiomassValue = 20f;
        public int EvolutionReward = 2;
        public bool IsPlayer;

        public event Action<LivingCell> Died;

        CircleCollider2D _col;
        float _contactCooldown;

        void OnEnable() => CellRegistry.Register(this);
        void OnDisable() => CellRegistry.Unregister(this);

        public void Configure(Faction faction, float radius, float maxHealth, float contactDamage, float biomassValue, int evoReward, bool isPlayer = false)
        {
            Faction = faction;
            Radius = radius;
            MaxHealth = maxHealth;
            Health = maxHealth;
            ContactDamage = contactDamage;
            BiomassValue = biomassValue;
            EvolutionReward = evoReward;
            IsPlayer = isPlayer;

            _col = GetComponent<CircleCollider2D>();
            _col.isTrigger = true;
            _col.radius = radius;
            transform.localScale = Vector3.one;
            CellRegistry.Register(this);
        }

        void Update()
        {
            if (_contactCooldown > 0f) _contactCooldown -= Time.deltaTime;

            // Keep player inside world soft boundary.
            var pos = transform.position;
            var dist = pos.magnitude;
            if (dist > GameConfig.WorldRadius)
            {
                transform.position = pos.normalized * GameConfig.WorldRadius;
            }
        }

        public void ApplyDamage(float amount, LivingCell source)
        {
            if (Health <= 0f) return;
            Health -= amount;
            if (Health <= 0f)
            {
                Health = 0f;
                HandleDeath(source);
            }
        }

        void HandleDeath(LivingCell source)
        {
            if (source != null && source.IsPlayer && GameState.Instance != null)
            {
                GameState.Instance.AddBiomass(BiomassValue);
                GameState.Instance.AddEvolutionPoints(EvolutionReward);
                if (Faction == Faction.Predator)
                    GameState.Instance.AddPopulation(3);
                else if (Faction == Faction.Prey)
                    GameState.Instance.AddPopulation(1);
            }

            Died?.Invoke(this);
            Destroy(gameObject);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (_contactCooldown > 0f) return;
            var otherCell = other.GetComponentInParent<LivingCell>();
            if (otherCell == null || otherCell == this) return;
            if (!CanHarm(Faction, otherCell.Faction)) return;

            var damage = ContactDamage;
            if (IsPlayer && GameState.Instance != null && GameState.Instance.HasSpikes)
                damage *= GameConfig.SpikesDamageBonus;

            otherCell.ReceiveContact(damage, this);
            _contactCooldown = 0.35f;
        }

        void ReceiveContact(float damage, LivingCell source)
        {
            if (IsPlayer && GameState.Instance != null)
            {
                GameState.Instance.Damage(damage);
                return;
            }

            ApplyDamage(damage, source);
        }

        public static bool CanHarm(Faction a, Faction b)
        {
            if (a == b) return false;
            if (a == Faction.Food || b == Faction.Food) return false;
            if ((a == Faction.Player || a == Faction.Ally) && (b == Faction.Player || b == Faction.Ally))
                return false;
            return true;
        }
    }
}
