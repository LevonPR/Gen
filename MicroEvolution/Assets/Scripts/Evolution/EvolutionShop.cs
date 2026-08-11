namespace MicroEvolution.Evolution
{
    public static class EvolutionShop
    {
        public static bool TryBuyOscillator() => PartCatalog.TryBuy(PartId.Oscillator);
        public static bool TryBuySpikes() => PartCatalog.TryBuy(PartId.Spikes);
        public static bool TryBuyMembrane() => PartCatalog.TryBuy(PartId.Membrane);
        public static bool TryBuyChemosynthesis() => PartCatalog.TryBuy(PartId.Chemosynthesis);
        public static bool TryBuyFlagella() => PartCatalog.TryBuy(PartId.Flagella);
        public static bool TryBuyEyes() => PartCatalog.TryBuy(PartId.Eyes);
        public static bool TryBuyJaws() => PartCatalog.TryBuy(PartId.Jaws);
        public static bool TryBuyToxin() => PartCatalog.TryBuy(PartId.Toxin);
        public static bool TryBuyStorage() => PartCatalog.TryBuy(PartId.Storage);
    }
}
