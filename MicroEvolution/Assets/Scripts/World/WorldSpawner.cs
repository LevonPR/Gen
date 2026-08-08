using MicroEvolution.AI;
using MicroEvolution.Core;
using MicroEvolution.Visuals;
using UnityEngine;

namespace MicroEvolution.World
{
    public class WorldSpawner : MonoBehaviour
    {
        Transform _foodRoot;
        Transform _microbeRoot;
        float _foodTimer;
        float _populationTimer;

        public void BuildEcology()
        {
            _foodRoot = new GameObject("FoodRoot").transform;
            _foodRoot.SetParent(transform, false);
            _microbeRoot = new GameObject("MicrobeRoot").transform;
            _microbeRoot.SetParent(transform, false);

            for (var i = 0; i < GameConfig.FoodCount; i++)
                FoodPellet.Spawn(_foodRoot, RandomPoint(4f));

            for (var i = 0; i < GameConfig.PreyCount; i++)
                SpawnMicrobe(Faction.Prey, RandomPoint(8f));

            for (var i = 0; i < GameConfig.PredatorCount; i++)
                SpawnMicrobe(Faction.Predator, RandomPoint(14f));

            for (var i = 0; i < GameConfig.AllyCount; i++)
                SpawnMicrobe(Faction.Ally, RandomPoint(6f));
        }

        void Update()
        {
            _foodTimer -= Time.deltaTime;
            if (_foodTimer <= 0f)
            {
                _foodTimer = 0.75f;
                if (_foodRoot != null && _foodRoot.childCount < GameConfig.FoodCount)
                    FoodPellet.Spawn(_foodRoot, RandomPoint(3f));
            }

            _populationTimer -= Time.deltaTime;
            if (_populationTimer <= 0f)
            {
                _populationTimer = GameConfig.PopulationTickSeconds;
                TickPopulation();
            }
        }

        void TickPopulation()
        {
            if (GameState.Instance == null) return;
            var allies = 0;
            var predators = 0;
            foreach (var cell in CellRegistry.All)
            {
                if (cell == null) continue;
                if (cell.Faction == Faction.Ally) allies++;
                if (cell.Faction == Faction.Predator) predators++;
            }

            var delta = Mathf.RoundToInt(allies * GameConfig.AllyPopulationContribution * 0.35f);
            delta += GameState.Instance.HasOscillator ? 1 : 0;
            delta -= predators > 6 ? 1 : 0;
            if (delta != 0) GameState.Instance.AddPopulation(delta);
        }

        void SpawnMicrobe(Faction faction, Vector2 pos)
        {
            var go = new GameObject(faction.ToString());
            go.transform.SetParent(_microbeRoot, false);
            go.transform.position = pos;

            go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>();
            var motor = go.AddComponent<CellMotor>();
            var cell = go.AddComponent<LivingCell>();
            var appearance = go.AddComponent<CellAppearance>();
            var ai = go.AddComponent<MicrobeAI>();

            float radius;
            float health;
            float damage;
            float speed;
            float biomass;
            int evo;
            Color color;
            float detect;

            switch (faction)
            {
                case Faction.Prey:
                    radius = 0.55f;
                    health = 35f;
                    damage = 0f;
                    speed = 4.2f;
                    biomass = GameConfig.PreyBiomassValue;
                    evo = GameConfig.PreyKillEvoReward;
                    color = new Color(0.55f, 0.85f, 1f, 0.78f);
                    detect = 9f;
                    break;
                case Faction.Predator:
                    radius = 1.15f;
                    health = 120f;
                    damage = 22f;
                    speed = 4.8f;
                    biomass = 70f;
                    evo = GameConfig.PredatorKillEvoReward;
                    color = new Color(0.18f, 0.14f, 0.2f, 0.92f);
                    detect = 12f;
                    break;
                default: // Ally
                    radius = 0.7f;
                    health = 55f;
                    damage = 10f;
                    speed = 5f;
                    biomass = 0f;
                    evo = 0;
                    color = new Color(0.45f, 0.95f, 0.75f, 0.8f);
                    detect = 10f;
                    break;
            }

            cell.Configure(faction, radius, health, damage, biomass, evo);
            appearance.Build(faction, radius, color);
            ai.Init(speed, detect);
            motor.MaxSpeed = speed;

            // Scale collider to match visual radius roughly.
            var col = go.GetComponent<CircleCollider2D>();
            col.radius = radius;
        }

        public static Vector2 RandomPoint(float minDistanceFromOrigin)
        {
            for (var i = 0; i < 20; i++)
            {
                var p = Random.insideUnitCircle * (GameConfig.WorldRadius * 0.9f);
                if (p.magnitude >= minDistanceFromOrigin) return p;
            }

            return Random.insideUnitCircle.normalized * minDistanceFromOrigin;
        }
    }
}
