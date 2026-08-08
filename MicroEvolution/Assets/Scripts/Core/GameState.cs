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
        public bool HasOscillator { get; private set; }
        public bool HasSpikes { get; private set; }
        public bool HasMembrane { get; private set; }
        public bool HasChemosynthesisUpgrade { get; private set; }
        public bool PopulationGoalMet { get; private set; }
        public bool OscillatorGoalMet { get; private set; }
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
            // Biomass slowly fuels population growth.
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

            GameEvents.RaiseStateChanged();
        }

        public void SetHealth(float value)
        {
            Health = Mathf.Clamp(value, 0f, MaxHealth);
            if (Health <= 0f && !PlayerDead)
            {
                PlayerDead = true;
                GameEvents.RaiseToast("Your cell ruptured. Press R to respawn.");
            }

            GameEvents.RaiseStateChanged();
        }

        public void Damage(float amount)
        {
            if (PlayerDead) return;
            var mitigated = HasMembrane ? amount * (1f - GameConfig.MembraneArmorBonus) : amount;
            SetHealth(Health - mitigated);
        }

        public void Heal(float amount)
        {
            if (PlayerDead) return;
            SetHealth(Health + amount);
        }

        public void UnlockOscillator()
        {
            if (HasOscillator) return;
            HasOscillator = true;
            OscillatorGoalMet = true;
            GameEvents.RaiseToast("Evolved Oscillator — swimming efficiency up");
            GameEvents.RaiseObjectivesChanged();
            CheckVictory();
            GameEvents.RaiseStateChanged();
        }

        public void UnlockSpikes()
        {
            HasSpikes = true;
            GameEvents.RaiseToast("Evolved Spikes — melee damage up");
            GameEvents.RaiseStateChanged();
        }

        public void UnlockMembrane()
        {
            HasMembrane = true;
            GameEvents.RaiseToast("Evolved Thick Membrane — damage taken down");
            GameEvents.RaiseStateChanged();
        }

        public void UnlockChemosynthesis()
        {
            HasChemosynthesisUpgrade = true;
            SetAtpMax(AtpMax + 30f);
            GameEvents.RaiseToast("Evolved Chemosynthesis — ATP capacity up");
            GameEvents.RaiseStateChanged();
        }

        public void RespawnPlayer()
        {
            PlayerDead = false;
            SetHealth(MaxHealth * 0.7f);
            Atp = AtpMax * 0.6f;
            Population = Mathf.Max(10, Population - 8);
            GameEvents.RaiseToast("Colony reformed a new cell");
            GameEvents.RaiseStateChanged();
        }

        void CheckVictory()
        {
            if (Victory) return;
            if (PopulationGoalMet && OscillatorGoalMet)
            {
                Victory = true;
                GameEvents.RaiseToast("Evolution milestone reached — Cell Stage complete!");
            }
        }
    }
}
