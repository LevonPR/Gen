using UnityEngine;

namespace MicroEvolution.Core
{
    [System.Serializable]
    public struct MetaSave
    {
        public int unlockedBiomeIndex;
        public int totalRuns;
        public int bestPopulation;
        public int unlockedPartsMask;
        public bool tutorialDone;
        public bool muted;
        public bool reducedParticles;
    }

    public static class SaveSystem
    {
        const string Key = "microevolution.meta.v1";

        public static void SaveMeta(GameFlow flow, GameState state)
        {
            var meta = LoadMeta();
            if (flow != null)
            {
                meta.unlockedBiomeIndex = Mathf.Max(meta.unlockedBiomeIndex, flow.UnlockedBiomeIndex);
                meta.totalRuns = Mathf.Max(meta.totalRuns, flow.CurrentRun);
            }

            if (state != null)
            {
                meta.bestPopulation = Mathf.Max(meta.bestPopulation, state.Population);
                meta.unlockedPartsMask |= PartsMask(state);
            }

            PlayerPrefs.SetString(Key, JsonUtility.ToJson(meta));
            PlayerPrefs.Save();
        }

        public static MetaSave LoadMeta()
        {
            if (!PlayerPrefs.HasKey(Key))
                return new MetaSave();

            try
            {
                return JsonUtility.FromJson<MetaSave>(PlayerPrefs.GetString(Key));
            }
            catch
            {
                return new MetaSave();
            }
        }

        public static void SetTutorialDone(bool done)
        {
            var meta = LoadMeta();
            meta.tutorialDone = done;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(meta));
            PlayerPrefs.Save();
        }

        public static void SetMuted(bool muted)
        {
            var meta = LoadMeta();
            meta.muted = muted;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(meta));
            PlayerPrefs.Save();
        }

        public static void SetReducedParticles(bool value)
        {
            var meta = LoadMeta();
            meta.reducedParticles = value;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(meta));
            PlayerPrefs.Save();
        }

        static int PartsMask(GameState state)
        {
            var mask = 0;
            if (state.HasOscillator) mask |= 1 << 0;
            if (state.HasSpikes) mask |= 1 << 1;
            if (state.HasMembrane) mask |= 1 << 2;
            if (state.HasChemosynthesisUpgrade) mask |= 1 << 3;
            if (state.HasFlagella) mask |= 1 << 4;
            if (state.HasEyes) mask |= 1 << 5;
            if (state.HasJaws) mask |= 1 << 6;
            if (state.HasToxin) mask |= 1 << 7;
            if (state.HasStorage) mask |= 1 << 8;
            return mask;
        }
    }
}
