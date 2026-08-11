using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Loads Meshy UI/cell PNGs from StreamingAssets or Art/Sprites.
    /// </summary>
    public static class ArtSpriteLibrary
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite GetUi(string nameNoExt)
        {
            return Load(nameNoExt, "Sprites/UI", "Art/Sprites/UI");
        }

        public static Sprite GetCell(string nameNoExt)
        {
            return Load(nameNoExt, "Sprites/Cells", "Art/Sprites/Cells");
        }

        static Sprite Load(string nameNoExt, string streamingSub, string artSub)
        {
            if (Cache.TryGetValue(nameNoExt, out var cached) && cached != null)
                return cached;

            var path = Resolve(nameNoExt + ".png", streamingSub, artSub);
            if (path == null) return null;

            try
            {
                var bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                if (!tex.LoadImage(bytes)) return null;
                tex.name = nameNoExt;
                var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                sprite.name = nameNoExt;
                Cache[nameNoExt] = sprite;
                return sprite;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ArtSpriteLibrary] {nameNoExt}: {e.Message}");
                return null;
            }
        }

        static string Resolve(string fileName, string streamingSub, string artSub)
        {
            var a = Path.Combine(Application.streamingAssetsPath, streamingSub, fileName);
            if (File.Exists(a)) return a;
            var b = Path.GetFullPath(Path.Combine(Application.dataPath, artSub, fileName));
            return File.Exists(b) ? b : null;
        }
    }
}
