using UnityEngine;

namespace MicroEvolution.Mobile
{
    public static class MobileSettings
    {
        public static bool IsMobileRuntime
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return true;
#else
                // Editor preview: enable with -mobile or force flag
                return Application.isMobilePlatform || ForceMobilePreview;
#endif
            }
        }

        public static bool ForceMobilePreview { get; set; }

        public static void ApplyRuntimeFlags()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (IsMobileRuntime)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                QualitySettings.vSyncCount = 0;
            }
        }
    }
}
