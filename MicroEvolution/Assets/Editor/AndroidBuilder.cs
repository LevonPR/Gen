#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MicroEvolution.EditorTools
{
    public static class AndroidBuilder
    {
        const string ProductName = "MicroEvolution";
        const string PackageName = "com.gen.microevolution";
        const string ApkPath = "Builds/Android/MicroEvolution.apk";

        [MenuItem("MicroEvolution/Configure Android Player Settings")]
        public static void ConfigureAndroid()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = "Gen";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.Android.bundleVersionCode = 2;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            Debug.Log("[MicroEvolution] Android player settings configured.");
        }

        [MenuItem("MicroEvolution/Build Android APK")]
        public static void BuildApkMenu()
        {
            var ok = BuildApk();
            EditorApplication.Exit(ok ? 0 : 1);
        }

        // CLI: -executeMethod MicroEvolution.EditorTools.AndroidBuilder.BuildApkCli
        public static void BuildApkCli()
        {
            var ok = BuildApk();
            EditorApplication.Exit(ok ? 0 : 1);
        }

        public static bool BuildApk()
        {
            ConfigureAndroid();
            Directory.CreateDirectory("Builds/Android");

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
            if (scenes.Length == 0)
                scenes = new[] { "Assets/Scenes/Main.unity" };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(options);
            var success = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
            if (success)
                Debug.Log($"[MicroEvolution] APK ready: {Path.GetFullPath(ApkPath)}");
            else
                Debug.LogError($"[MicroEvolution] APK build failed: {report.summary.result}");
            return success;
        }
    }
}
#endif
