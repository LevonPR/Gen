namespace MicroEvolution.Core
{
    public static class GameConfig
    {
        public const float WorldRadius = 55f;
        public const float CameraSize = 12f;

        public const float PlayerMaxHealth = 100f;
        public const float PlayerBaseSpeed = 6.5f;
        public const float PlayerBoostMultiplier = 1.85f;
        public const float PlayerRadius = 0.85f;

        public const float AtpMaxBase = 110f;
        public const float AtpRegenPerSecond = 8f;
        public const float MoveAtpCostPerSecond = 4f;
        public const float BoostAtpCostPerSecond = 22f;

        public const int StartingPopulation = 40;
        public const int PopulationGoal = 150;
        public const float PopulationTickSeconds = 2.5f;
        public const float AllyPopulationContribution = 1.2f;

        public const int StartingEvolutionPoints = 8;
        public const int FoodEvoReward = 1;
        public const int PreyKillEvoReward = 3;
        public const int PredatorKillEvoReward = 6;

        public const float FoodBiomassValue = 12f;
        public const float PreyBiomassValue = 35f;

        public const int FoodCount = 70;
        public const int PreyCount = 18;
        public const int PredatorCount = 8;
        public const int AllyCount = 10;

        public const float ChemosynthesisCooldown = 12f;
        public const float ChemosynthesisAtpRestore = 45f;
        public const float ChemosynthesisHeal = 18f;

        public const float OscillatorSpeedBonus = 1.2f;
        public const float SpikesDamageBonus = 1.5f;
        public const float MembraneArmorBonus = 0.35f;
    }
}
