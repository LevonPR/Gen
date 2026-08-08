using MicroEvolution.AI;
using MicroEvolution.Core;
using MicroEvolution.Mobile;
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

            var food = MobileSettings.IsMobileRuntime ? GameConfig.FoodCountMobile : GameConfig.FoodCount;
            var prey = MobileSettings.IsMobileRuntime ? GameConfig.PreyCountMobile : GameConfig.PreyCount;
            var predators = MobileSettings.IsMobileRuntime ? GameConfig.PredatorCountMobile : GameConfig.PredatorCount;
            var allies = MobileSettings.IsMobileRuntime ? GameConfig.AllyCountMobile : GameConfig.AllyCount;

            for (var i = 0; i < food; i++)
                FoodPellet.Spawn(_foodRoot, RandomPoint(4f));

            for (var i = 0; i < prey; i++)
                SpawnMicrobe(Faction.Prey, RandomPoint(8f));

            for (var i = 0; i < predators; i++)
                SpawnMicrobe(Faction.Predator, RandomPoint(18f));

            for (var i = 0; i < allies; i++)
                SpawnMicrobe(Faction.Ally, RandomPoint(6f));
        }

        void Update()
        {
            if (GameFlow.Instance != null && GameFlow.Instance.Screen != AppScreen.Playing) return;

            _foodTimer -= Time.deltaTime;
            if (_foodTimer <= 0f)
            {
                _foodTimer = 0.7f;
                var cap = MobileSettings.IsMobileRuntime ? GameConfig.FoodCountMobile : GameConfig.FoodCount;
                if (_foodRoot != null && _foodRoot.childCount < cap)
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
            delta += GameState.Instance.HasStorage ? 1 : 0;
            delta -= predators > 6 ? 1 : 0;
            if (delta != 0) GameState.Instance.AddPopulation(delta);
        }

        void SpawnMicrobe(Faction faction, Vector2 pos)
        {
            // Bias predators outward toward vents.
            if (faction == Faction.Predator && pos.magnitude < GameConfig.TidePoolRadius)
                pos = pos.normalized * Random.Range(GameConfig.TidePoolRadius, GameConfig.WorldRadius * 0.85f);

            var go = new GameObject(faction.ToString());
            go.transform.SetParent(_microbeRoot, false);
            go.transform.position = pos;

            go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>();
            var motor = go.AddComponent<CellMotor>();
            var cell = go.AddComponent<LivingCell>();
            var appearance = go.AddComponent<CellAppearance>();
            var ai = go.AddComponent<MicrobeAI>();

            float radius, health, damage, speed, biomass, detect;
            int evo;
            Color color;

            var vent = pos.magnitude >= GameConfig.MidwaterRadius;

            switch (faction)
            {
                case Faction.Prey:
                    radius = vent ? 0.5f : 0.55f;
                    health = vent ? 28f : 35f;
                    damage = 0f;
                    speed = vent ? 4.8f : 4.2f;
                    biomass = GameConfig.PreyBiomassValue * (vent ? 1.3f : 1f);
                    evo = GameConfig.PreyKillEvoReward;
                    color = vent
                        ? new Color(1f, 0.55f, 0.25f, 0.82f)
                        : Color.Lerp(new Color(0.55f, 0.85f, 1f, 0.78f), new Color(1f, 0.75f, 0.35f, 0.8f), Random.value);
                    detect = 9f;
                    break;
                case Faction.Predator:
                    radius = vent ? 1.35f : 1.15f;
                    health = vent ? 150f : 120f;
                    damage = vent ? 28f : 22f;
                    speed = vent ? 5.1f : 4.8f;
                    biomass = vent ? 90f : 70f;
                    evo = GameConfig.PredatorKillEvoReward + (vent ? 2 : 0);
                    color = vent
                        ? new Color(0.85f, 0.12f, 0.18f, 0.92f)
                        : new Color(0.55f, 0.15f, 0.65f, 0.9f);
                    detect = 12f;
                    break;
                default:
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
            go.GetComponent<CircleCollider2D>().radius = radius;
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
