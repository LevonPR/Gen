using UnityEngine;

namespace MicroEvolution.Core
{
    public enum AppScreen
    {
        MainMenu,
        Playing,
        Paused,
        Victory,
        GameOver
    }

    public class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }

        public AppScreen Screen { get; private set; } = AppScreen.MainMenu;
        public int CurrentRun { get; private set; } = 1;
        public int UnlockedBiomeIndex { get; private set; }

        public System.Action ScreenChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void ShowMainMenu()
        {
            Time.timeScale = 1f;
            SetScreen(AppScreen.MainMenu);
        }

        public void StartNewRun()
        {
            Time.timeScale = 1f;
            CurrentRun++;
            SetScreen(AppScreen.Playing);
            GameEvents.RaiseToast("New run — survive and evolve");
        }

        public void Resume()
        {
            if (Screen != AppScreen.Paused) return;
            Time.timeScale = 1f;
            SetScreen(AppScreen.Playing);
        }

        public void Pause()
        {
            if (Screen != AppScreen.Playing) return;
            Time.timeScale = 0f;
            SetScreen(AppScreen.Paused);
        }

        public void TogglePause()
        {
            if (Screen == AppScreen.Playing) Pause();
            else if (Screen == AppScreen.Paused) Resume();
        }

        public void AnnounceVictory()
        {
            if (Screen == AppScreen.Victory) return;
            Time.timeScale = 0.35f;
            UnlockedBiomeIndex = Mathf.Max(UnlockedBiomeIndex, 2);
            SaveSystem.SaveMeta(this, GameState.Instance);
            SetScreen(AppScreen.Victory);
        }

        public void AnnounceGameOver()
        {
            // Soft game-over: colony can still respawn; hard fail if population collapses.
            if (GameState.Instance != null && GameState.Instance.Population <= 0)
            {
                Time.timeScale = 0f;
                SetScreen(AppScreen.GameOver);
            }
        }

        public void LoadMeta()
        {
            var meta = SaveSystem.LoadMeta();
            UnlockedBiomeIndex = meta.unlockedBiomeIndex;
            CurrentRun = Mathf.Max(1, meta.totalRuns);
        }

        void SetScreen(AppScreen screen)
        {
            Screen = screen;
            ScreenChanged?.Invoke();
            GameEvents.RaiseStateChanged();
        }
    }
}
