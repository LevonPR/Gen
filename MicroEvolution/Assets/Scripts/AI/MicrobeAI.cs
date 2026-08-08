using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.AI
{
    [RequireComponent(typeof(CellMotor))]
    [RequireComponent(typeof(LivingCell))]
    public class MicrobeAI : MonoBehaviour
    {
        public float DetectRange = 10f;
        public float WanderChangeSeconds = 2.2f;

        CellMotor _motor;
        LivingCell _cell;
        Vector2 _wanderDir;
        float _wanderTimer;
        float _repathTimer;

        public void Init(float speed, float detectRange)
        {
            _motor = GetComponent<CellMotor>();
            _cell = GetComponent<LivingCell>();
            _motor.MaxSpeed = speed;
            DetectRange = detectRange;
            PickWander();
        }

        void Update()
        {
            if (_cell == null || _motor == null) return;

            _repathTimer -= Time.deltaTime;
            if (_repathTimer <= 0f)
            {
                _repathTimer = 0.25f;
                Think();
            }
        }

        void Think()
        {
            Transform target = null;
            var flee = false;

            switch (_cell.Faction)
            {
                case Faction.Prey:
                {
                    var food = CellRegistry.FindNearest(transform.position, DetectRange * 0.8f,
                        c => c.Faction == Faction.Food);
                    // Food pellets are not LivingCells; prey just wanders / flees.
                    var threat = CellRegistry.FindNearest(transform.position, DetectRange,
                        c => c != _cell && (c.Faction == Faction.Player || c.Faction == Faction.Predator));
                    if (threat != null)
                    {
                        target = threat.transform;
                        flee = true;
                    }
                    else if (food != null)
                    {
                        target = food.transform;
                    }

                    break;
                }
                case Faction.Predator:
                {
                    var prey = CellRegistry.FindNearest(transform.position, DetectRange,
                        c => c != _cell && (c.Faction == Faction.Player || c.Faction == Faction.Prey || c.Faction == Faction.Ally));
                    if (prey != null) target = prey.transform;
                    break;
                }
                case Faction.Ally:
                {
                    var enemy = CellRegistry.FindNearest(transform.position, DetectRange * 0.9f,
                        c => c != _cell && c.Faction == Faction.Predator);
                    if (enemy != null) target = enemy.transform;
                    break;
                }
            }

            if (target != null)
            {
                var dir = (Vector2)(target.position - transform.position);
                if (flee) dir = -dir;
                _motor.SetDesiredVelocity(dir.normalized * _motor.MaxSpeed);
                return;
            }

            _wanderTimer -= Time.deltaTime;
            if (_wanderTimer <= 0f) PickWander();

            var foods = Physics2D.OverlapCircleAll(transform.position, DetectRange * 0.7f);
            Transform nearestFood = null;
            var best = float.MaxValue;
            foreach (var hit in foods)
            {
                if (hit.GetComponent<World.FoodPellet>() == null) continue;
                var d = (hit.transform.position - transform.position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    nearestFood = hit.transform;
                }
            }

            if (nearestFood != null && (_cell.Faction == Faction.Prey || _cell.Faction == Faction.Ally))
            {
                var dir = (Vector2)(nearestFood.position - transform.position);
                _motor.SetDesiredVelocity(dir.normalized * _motor.MaxSpeed);
                return;
            }

            _motor.SetDesiredVelocity(_wanderDir * (_motor.MaxSpeed * 0.65f));
        }

        void PickWander()
        {
            _wanderDir = Random.insideUnitCircle.normalized;
            if (_wanderDir.sqrMagnitude < 0.01f) _wanderDir = Vector2.right;
            _wanderTimer = WanderChangeSeconds * Random.Range(0.7f, 1.4f);
        }
    }
}
