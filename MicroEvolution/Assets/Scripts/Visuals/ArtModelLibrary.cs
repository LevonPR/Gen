using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Loads authored Meshy GLBs from StreamingAssets/Models (or Art/Models/Cells in Editor).
    /// </summary>
    public static class ArtModelLibrary
    {
        static readonly Dictionary<string, GameObject> Templates = new Dictionary<string, GameObject>();
        static readonly HashSet<string> Failed = new HashSet<string>();
        static bool _warming;

        public static string FileForStyle(MicrobeStyle style)
        {
            switch (style)
            {
                case MicrobeStyle.Eukaryote: return "player_core";
                case MicrobeStyle.RodBacteria: return "prey_rod";
                case MicrobeStyle.SpikyOrb: return "predator_spiky";
                case MicrobeStyle.Segmented: return "predator_worm";
                case MicrobeStyle.AllyProbe: return "ally_probe";
                default: return null;
            }
        }

        public static async void WarmupAsync(params string[] names)
        {
            if (_warming) return;
            _warming = true;
            try
            {
                foreach (var n in names)
                    await EnsureTemplateAsync(n);
            }
            finally
            {
                _warming = false;
            }
        }

        public static bool TryAttach(Transform parent, MicrobeStyle style, float radius, out Transform body)
        {
            body = null;
            var file = FileForStyle(style);
            if (string.IsNullOrEmpty(file)) return false;
            if (!Templates.TryGetValue(file, out var template) || template == null)
                return false;

            var go = UnityEngine.Object.Instantiate(template, parent);
            go.name = "ArtBody_" + file;
            go.SetActive(true);
            NormalizeScale(go.transform, radius);
            body = go.transform;
            return true;
        }

        public static async Task<bool> EnsureTemplateAsync(string fileNameNoExt)
        {
            if (Templates.ContainsKey(fileNameNoExt) && Templates[fileNameNoExt] != null)
                return true;
            if (Failed.Contains(fileNameNoExt))
                return false;

            var path = ResolvePath(fileNameNoExt);
            if (path == null)
            {
                Failed.Add(fileNameNoExt);
                return false;
            }

            try
            {
                var bytes = File.ReadAllBytes(path);
                var gltf = new GltfImport();
                var uri = new Uri("file://" + path.Replace("\\", "/"));
                var ok = await gltf.Load(bytes, uri);
                if (!ok)
                {
                    Failed.Add(fileNameNoExt);
                    return false;
                }

                var holder = new GameObject("GLBTemplate_" + fileNameNoExt);
                holder.hideFlags = HideFlags.HideAndDontSave;
                holder.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(holder);
                ok = await gltf.InstantiateMainSceneAsync(holder.transform);
                if (!ok)
                {
                    UnityEngine.Object.Destroy(holder);
                    Failed.Add(fileNameNoExt);
                    return false;
                }

                Templates[fileNameNoExt] = holder;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ArtModelLibrary] Failed to load {fileNameNoExt}: {e.Message}");
                Failed.Add(fileNameNoExt);
                return false;
            }
        }

        static string ResolvePath(string fileNameNoExt)
        {
            var streaming = Path.Combine(Application.streamingAssetsPath, "Models", fileNameNoExt + ".glb");
            if (File.Exists(streaming)) return streaming;
            var art = Path.GetFullPath(Path.Combine(Application.dataPath, "Art/Models/Cells", fileNameNoExt + ".glb"));
            if (File.Exists(art)) return art;
            return null;
        }

        static void NormalizeScale(Transform root, float radius)
        {
            var rends = root.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0)
            {
                root.localScale = Vector3.one * (radius * 2f);
                return;
            }

            var bounds = rends[0].bounds;
            for (var i = 1; i < rends.Length; i++)
                bounds.Encapsulate(rends[i].bounds);

            var max = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (max < 0.0001f) max = 1f;
            var target = radius * 2.05f;
            var s = target / max;
            root.localScale = Vector3.one * s;
            var localCenter = root.InverseTransformPoint(bounds.center);
            root.localPosition = -localCenter * s;
        }
    }
}
