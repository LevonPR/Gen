namespace MicroEvolution.Core
{
    public static class GameConfig
    {
        public const float WorldRadius = 70f;
        public const float CameraSize = 11.5f;
        public const float CameraSizeMobile = 10.5f;

        public const float PlayerMaxHealth = 100f;
        public const float PlayerBaseSpeed = 6.5f;
        public const float PlayerBoostMultiplier = 1.85f;
        public const float PlayerRadius = 0.85f;

        public const float AtpMaxBase = 110f;
        public const float AtpRegenPerSecond = 9f;
        public const float MoveAtpCostPerSecond = 3.5f;
        public const float BoostAtpCostPerSecond = 20f;

        public const int StartingPopulation = 45;
        public const int PopulationGoal = 150;
        public const float PopulationTickSeconds = 2.4f;
        public const float AllyPopulationContribution = 1.2f;

        public const int StartingEvolutionPoints = 10;
        public const int FoodEvoReward = 1;
        public const int PreyKillEvoReward = 3;
        public const int PredatorKillEvoReward = 6;

        public const float FoodBiomassValue = 12f;
        public const float PreyBiomassValue = 35f;

        public const int FoodCount = 80;
        public const int PreyCount = 20;
        public const int PredatorCount = 9;
        public const int AllyCount = 12;

        public const int FoodCountMobile = 55;
        public const int PreyCountMobile = 14;
        public const int PredatorCountMobile = 7;
        public const int AllyCountMobile = 9;

        public const float ChemosynthesisCooldown = 11f;
        public const float ChemosynthesisAtpRestore = 45f;
        public const float ChemosynthesisHeal = 18f;

        public const float OscillatorSpeedBonus = 1.18f;
        public const float FlagellaSpeedBonus = 1.12f;
        public const float SpikesDamageBonus = 1.45f;
        public const float JawsBiomassBonus = 1.35f;
        public const float MembraneArmorBonus = 0.35f;
        public const float ToxinDps = 8f;

        public const float TidePoolRadius = 22f;
        public const float MidwaterRadius = 45f;
        // Beyond MidwaterRadius → Thermal Vent
    }
}
