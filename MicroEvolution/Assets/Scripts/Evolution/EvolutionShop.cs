using MicroEvolution.Core;

namespace MicroEvolution.Evolution
{
    public static class EvolutionShop
    {
        public const int OscillatorCost = 12;
        public const int SpikesCost = 10;
        public const int MembraneCost = 10;
        public const int ChemosynthesisCost = 14;

        public static bool TryBuyOscillator()
        {
            var state = GameState.Instance;
            if (state == null || state.HasOscillator) return false;
            if (!state.TrySpendEvolutionPoints(OscillatorCost))
            {
                GameEvents.RaiseToast($"Need {OscillatorCost} evo points for Oscillator");
                return false;
            }

            state.UnlockOscillator();
            return true;
        }

        public static bool TryBuySpikes()
        {
            var state = GameState.Instance;
            if (state == null || state.HasSpikes) return false;
            if (!state.TrySpendEvolutionPoints(SpikesCost))
            {
                GameEvents.RaiseToast($"Need {SpikesCost} evo points for Spikes");
                return false;
            }

            state.UnlockSpikes();
            return true;
        }

        public static bool TryBuyMembrane()
        {
            var state = GameState.Instance;
            if (state == null || state.HasMembrane) return false;
            if (!state.TrySpendEvolutionPoints(MembraneCost))
            {
                GameEvents.RaiseToast($"Need {MembraneCost} evo points for Membrane");
                return false;
            }

            state.UnlockMembrane();
            return true;
        }

        public static bool TryBuyChemosynthesis()
        {
            var state = GameState.Instance;
            if (state == null || state.HasChemosynthesisUpgrade) return false;
            if (!state.TrySpendEvolutionPoints(ChemosynthesisCost))
            {
                GameEvents.RaiseToast($"Need {ChemosynthesisCost} evo points for Chemosynthesis");
                return false;
            }

            state.UnlockChemosynthesis();
            return true;
        }
    }
}
