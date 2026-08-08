using UnityEngine;

namespace MicroEvolution.Core
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public float Atp { get; private set; } = GameConfig.AtpMaxBase;
        public float AtpMax { get; private set; } = GameConfig.AtpMaxBase;
        public float Biomass { get; private set; }
        public int EvolutionPoints { get; private set; } = GameConfig.StartingEvolutionPoints;
        public int Population { get; private set; } = GameConfig.StartingPopulation;
        public float Health { get; private set; } = GameConfig.PlayerMaxHealth;
        public float MaxHealth { get; private set; } = GameConfig.PlayerMaxHealth;
        public int CurrentBiomeIndex { get; private set; }
        public int PredatorsKilled { get; private set; }
        public int FoodEaten { get; private set; }

        public bool HasOscillator { get; private set; }
        public bool HasSpikes { get; private set; }
        public bool HasMembrane { get; private set; }
        public bool HasChemosynthesisUpgrade { get; private set; }
        public bool HasFlagella { get; private set; }
        public bool HasEyes { get; private set; }
        public bool HasJaws { get; private set; }
        public bool HasToxin { get; private set; }
        public bool HasStorage { get; private set; }

        public bool PopulationGoalMet { get; private set; }
        public bool OscillatorGoalMet { get; private set; }
        public bool BiomeGoalMet { get; private set; }
        public bool Victory { get; private set; }
        public bool PlayerDead { get; private set; }

        public Transform PlayerTransform { get; set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ApplyMetaBonuses();
        }

        void ApplyMetaBonuses()
        {
            var meta = SaveSystem.LoadMeta();
            if (meta.unlockedBiomeIndex >= 1)
                EvolutionPoints += 4;
            if (meta.bestPopulation >= 100)
                AtpMax += 15f;
            Atp = AtpMax;
        }

        public void ResetRun()
        {
            AtpMax = GameConfig.AtpMaxBase;
            ApplyMetaBonuses();
            Atp = AtpMax;
            Biomass = 0f;
            EvolutionPoints = GameConfig.StartingEvolutionPoints + (SaveSystem.LoadMeta().unlockedBiomeIndex >= 1 ? 4 : 0);
            Population = GameConfig.StartingPopulation;
            MaxHealth = GameConfig.PlayerMaxHealth;
            Health = MaxHealth;
            CurrentBiomeIndex = 0;
            PredatorsKilled = 0;
            FoodEaten = 0;
            HasOscillator = HasSpikes = HasMembrane = HasChemosynthesisUpgrade = false;
            HasFlagella = HasEyes = HasJaws = HasToxin = HasStorage = false;
            PopulationGoalMet = OscillatorGoalMet = BiomeGoalMet = Victory = PlayerDead = false;
            GameEvents.RaiseStateChanged();
            GameEvents.RaiseObjectivesChanged();
        }

        public void SetBiomeIndex(int index)
        {
            if (index == CurrentBiomeIndex) return;
            CurrentBiomeIndex = index;
            if (!BiomeGoalMet && index >= 2)
            {
                BiomeGoalMet = true;
                GameEvents.RaiseToast("Objective: reached Thermal Vent biome");
                GameEvents.RaiseObjectivesChanged();
                CheckVictory();
            }

            GameEvents.RaiseStateChanged();
        }

        public void SetAtpMax(float value)
        {
            AtpMax = value;
            Atp = Mathf.Min(Atp, AtpMax);
            GameEvents.RaiseStateChanged();
        }

        public void AddAtp(float amount)
        {
            Atp = Mathf.Clamp(Atp + amount, 0f, AtpMax);
            GameEvents.RaiseStateChanged();
        }

        public bool TrySpendAtp(float amount)
        {
            if (Atp < amount) return false;
            Atp -= amount;
            GameEvents.RaiseStateChanged();
            return true;
        }

        public void AddBiomass(float amount)
        {
            Biomass += amount;
            FoodEaten++;
            var bonus = Mathf.FloorToInt(amount / 40f);
            if (bonus > 0) AddPopulation(bonus);
            GameEvents.RaiseStateChanged();
        }

        public void AddEvolutionPoints(int amount)
        {
            EvolutionPoints += amount;
            GameEvents.RaiseStateChanged();
        }

        public bool TrySpendEvolutionPoints(int amount)
        {
            if (EvolutionPoints < amount) return false;
            EvolutionPoints -= amount;
            GameEvents.RaiseStateChanged();
            return true;
        }

        public void AddPopulation(int amount)
        {
            Population = Mathf.Max(0, Population + amount);
            if (!PopulationGoalMet && Population >= GameConfig.PopulationGoal)
            {
                PopulationGoalMet = true;
                GameEvents.RaiseToast("Objective complete: Reach population 150");
                GameEvents.RaiseObjectivesChanged();
                CheckVictory();
            }

            if (Population <= 0)
                GameFlow.Instance?.AnnounceGameOver();

            GameEvents.RaiseStateChanged();
            SaveSystem.SaveMeta(GameFlow.Instance, this);
        }

        public void SetHealth(float value)
        {
            Health = Mathf.Clamp(value, 0f, MaxHealth);
            if (Health <= 0f && !PlayerDead)
            {
                PlayerDead = true;
                AddPopulation(-8);
                GameEvents.RaiseToast("Your cell ruptured. Respawn from colony.");
            }

            GameEvents.RaiseStateChanged();
        }

        public void Damage(float amount)
        {
            if (PlayerDead) return;
            var mitigated = HasMembrane ? amount * (1f - GameConfig.MembraneArmorBonus) : amount;
            if (HasStorage) mitigated *= 0.92f;
            SetHealth(Health - mitigated);
            GameEvents.RaisePlayerHurt(mitigated);
        }

        public void Heal(float amount)
        {
            if (PlayerDead) return;
            SetHealth(Health + amount);
        }

        public void RegisterPredatorKill()
        {
            PredatorsKilled++;
            AddPopulation(3);
        }

        public void UnlockOscillator()
        {
            if (HasOscillator) return;
            HasOscillator = true;
            OscillatorGoalMet = true;
            GameEvents.RaiseToast("Evolved Oscillator");
            GameEvents.RaiseObjectivesChanged();
            CheckVictory();
            AfterUnlock();
        }

        public void UnlockSpikes() { if (HasSpikes) return; HasSpikes = true; GameEvents.RaiseToast("Evolved Spikes"); AfterUnlock(); }
        public void UnlockMembrane() { if (HasMembrane) return; HasMembrane = true; GameEvents.RaiseToast("Evolved Thick Membrane"); AfterUnlock(); }

        public void UnlockChemosynthesis()
        {
            if (HasChemosynthesisUpgrade) return;
            HasChemosynthesisUpgrade = true;
            SetAtpMax(AtpMax + 30f);
            GameEvents.RaiseToast("Evolved Chemosynthesis");
            AfterUnlock();
        }

        public void UnlockFlagella()
        {
            if (HasFlagella) return;
            HasFlagella = true;
            GameEvents.RaiseToast("Evolved Flagella — turn & cruise speed up");
            AfterUnlock();
        }

        public void UnlockEyes()
        {
            if (HasEyes) return;
            HasEyes = true;
            GameEvents.RaiseToast("Evolved Eyes — vent darkness cleared");
            AfterUnlock();
        }

        public void UnlockJaws()
        {
            if (HasJaws) return;
            HasJaws = true;
            GameEvents.RaiseToast("Evolved Jaws — bonus biomass from kills");
            AfterUnlock();
        }

        public void UnlockToxin()
        {
            if (HasToxin) return;
            HasToxin = true;
            GameEvents.RaiseToast("Evolved Toxin Glands — damage over time");
            AfterUnlock();
        }

        public void UnlockStorage()
        {
            if (HasStorage) return;
            HasStorage = true;
            SetAtpMax(AtpMax + 20f);
            GameEvents.RaiseToast("Evolved Vacuole Storage — ATP capacity up");
            AfterUnlock();
        }

        void AfterUnlock()
        {
            GameEvents.RaiseStateChanged();
            SaveSystem.SaveMeta(GameFlow.Instance, this);
            GameEvents.RaiseEvolve();
        }

        public void RespawnPlayer()
        {
            if (Population <= 0) return;
            PlayerDead = false;
            SetHealth(MaxHealth * 0.7f);
            Atp = AtpMax * 0.6f;
            GameEvents.RaiseToast("Colony reformed a new cell");
            GameEvents.RaiseStateChanged();
        }

        void CheckVictory()
        {
            if (Victory) return;
            // Full victory: population + oscillator + reached deepest biome
            if (PopulationGoalMet && OscillatorGoalMet && BiomeGoalMet)
            {
                Victory = true;
                GameEvents.RaiseToast("Cell Stage complete!");
                GameFlow.Instance?.AnnounceVictory();
            }
        }
    }
}
