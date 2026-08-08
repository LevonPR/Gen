using System;
using MicroEvolution.Visuals;
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
        SpriteRenderer _flashTarget;
        float _flash;

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

            var pos = transform.position;
            var dist = pos.magnitude;
            if (dist > GameConfig.WorldRadius)
                transform.position = pos.normalized * GameConfig.WorldRadius;

            if (_flash > 0f)
            {
                _flash -= Time.deltaTime * 5f;
                if (_flashTarget == null)
                {
                    var body = transform.Find("Body");
                    if (body != null) _flashTarget = body.GetComponent<SpriteRenderer>();
                }

                if (_flashTarget != null)
                {
                    var baseColor = _flashTarget.color;
                    baseColor.a = 0.72f;
                    _flashTarget.color = Color.Lerp(baseColor, Color.white, Mathf.Clamp01(_flash));
                }
            }
        }

        public void ApplyDamage(float amount, LivingCell source)
        {
            if (Health <= 0f) return;
            Health -= amount;
            _flash = 1f;
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
                var biomass = BiomassValue;
                if (GameState.Instance.HasJaws) biomass *= GameConfig.JawsBiomassBonus;
                GameState.Instance.AddBiomass(biomass);
                GameState.Instance.AddEvolutionPoints(EvolutionReward);
                if (Faction == Faction.Predator)
                {
                    GameState.Instance.RegisterPredatorKill();
                    GameEvents.RaiseKillPulse();
                    FloatingText.Spawn(transform.position, "Predator down", new Color(1f, 0.5f, 0.4f));
                }
                else if (Faction == Faction.Prey)
                {
                    GameState.Instance.AddPopulation(1);
                    FloatingText.Spawn(transform.position, $"+{biomass:0}", new Color(1f, 0.9f, 0.4f));
                }
            }

            VfxBurst.Spawn(transform.position,
                Faction == Faction.Predator ? new Color(1f, 0.3f, 0.25f, 0.85f) : new Color(0.6f, 0.9f, 1f, 0.8f),
                Radius * 1.8f);

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
                CameraImpulse.Instance?.Punch(0.18f);
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
