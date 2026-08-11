using System;
using MicroEvolution.Core;

namespace MicroEvolution.Evolution
{
    public enum PartId
    {
        Oscillator = 0,
        Spikes = 1,
        Membrane = 2,
        Chemosynthesis = 3,
        Flagella = 4,
        Eyes = 5,
        Jaws = 6,
        Toxin = 7,
        Storage = 8
    }

    public readonly struct PartDef
    {
        public readonly PartId Id;
        public readonly string Name;
        public readonly int Cost;
        public readonly string Description;
        public readonly Func<GameState, bool> IsOwned;
        public readonly Action<GameState> Unlock;

        public PartDef(PartId id, string name, int cost, string description, Func<GameState, bool> isOwned, Action<GameState> unlock)
        {
            Id = id;
            Name = name;
            Cost = cost;
            Description = description;
            IsOwned = isOwned;
            Unlock = unlock;
        }
    }

    public static class PartCatalog
    {
        public static readonly PartDef[] All =
        {
            new PartDef(PartId.Oscillator, "Oscillator", 12, "Speed + objective part",
                s => s.HasOscillator, s => s.UnlockOscillator()),
            new PartDef(PartId.Spikes, "Spikes", 10, "Melee damage up",
                s => s.HasSpikes, s => s.UnlockSpikes()),
            new PartDef(PartId.Membrane, "Membrane", 10, "Take less damage",
                s => s.HasMembrane, s => s.UnlockMembrane()),
            new PartDef(PartId.Chemosynthesis, "Chemosynthesis", 14, "Stronger E ability + ATP max",
                s => s.HasChemosynthesisUpgrade, s => s.UnlockChemosynthesis()),
            new PartDef(PartId.Flagella, "Flagella", 11, "Cruise & turn speed up",
                s => s.HasFlagella, s => s.UnlockFlagella()),
            new PartDef(PartId.Eyes, "Eyes", 9, "See clearly in Thermal Vent",
                s => s.HasEyes, s => s.UnlockEyes()),
            new PartDef(PartId.Jaws, "Jaws", 13, "More biomass from kills",
                s => s.HasJaws, s => s.UnlockJaws()),
            new PartDef(PartId.Toxin, "Toxin", 15, "Damage nearby enemies over time",
                s => s.HasToxin, s => s.UnlockToxin()),
            new PartDef(PartId.Storage, "Storage", 12, "ATP capacity + minor armor",
                s => s.HasStorage, s => s.UnlockStorage()),
        };

        public static bool TryBuy(PartId id)
        {
            var state = GameState.Instance;
            if (state == null) return false;
            foreach (var part in All)
            {
                if (part.Id != id) continue;
                if (part.IsOwned(state)) return false;
                if (!state.TrySpendEvolutionPoints(part.Cost))
                {
                    GameEvents.RaiseToast($"Need {part.Cost} evo for {part.Name}");
                    return false;
                }

                part.Unlock(state);
                return true;
            }

            return false;
        }
    }
}
