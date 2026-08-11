#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MicroEvolution.EditorTools
{
    /// <summary>
    /// Copies Art/Models/Cells/*.glb into StreamingAssets/Models for runtime loading.
    /// </summary>
    public static class ArtStreamingSync
    {
        const string Src = "Assets/Art/Models/Cells";
        const string Dst = "Assets/StreamingAssets/Models";

        [MenuItem("MicroEvolution/Sync Art GLBs to StreamingAssets")]
        public static void Sync()
        {
            Directory.CreateDirectory(Dst);
            var count = 0;
            foreach (var path in Directory.GetFiles(Src, "*.glb"))
            {
                var name = Path.GetFileName(path);
                var dest = Path.Combine(Dst, name);
                File.Copy(path, dest, true);
                count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[MicroEvolution] Synced {count} GLB(s) to {Dst}");
        }

        [InitializeOnLoadMethod]
        static void AutoSyncOnLoad()
        {
            if (!Directory.Exists(Src)) return;
            Directory.CreateDirectory(Dst);
            foreach (var path in Directory.GetFiles(Src, "*.glb"))
            {
                var dest = Path.Combine(Dst, Path.GetFileName(path));
                if (!File.Exists(dest) || File.GetLastWriteTimeUtc(path) > File.GetLastWriteTimeUtc(dest))
                    File.Copy(path, dest, true);
            }
        }
    }
}
#endif
