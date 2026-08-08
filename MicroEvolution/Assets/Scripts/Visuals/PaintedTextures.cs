using System.Collections.Generic;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Higher-res noise "paintings" for biome plates — procedural stand-in for hand art.
    /// </summary>
    public static class PaintedTextures
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite BiomePlate(string key, Color a, Color b, Color accent, int size = 256)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var u = x / (float)(size - 1);
                var v = y / (float)(size - 1);
                var n1 = Mathf.PerlinNoise(u * 4.2f + 3.1f, v * 4.2f + 7.7f);
                var n2 = Mathf.PerlinNoise(u * 9.5f + 11f, v * 9.5f + 2f);
                var n3 = Mathf.PerlinNoise(u * 18f, v * 18f + 5f);
                var mottled = Mathf.Clamp01(n1 * 0.55f + n2 * 0.3f + n3 * 0.15f);
                var col = Color.Lerp(a, b, mottled);
                // Vein / organic streak accents
                var vein = Mathf.Abs(Mathf.Sin((u + n2) * 20f) * Mathf.Cos((v + n1) * 14f));
                if (vein > 0.92f) col = Color.Lerp(col, accent, 0.55f);
                // Soft circular mask
                var dx = u - 0.5f;
                var dy = v - 0.5f;
                var dist = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                var alpha = Mathf.Clamp01(1f - Mathf.SmoothStep(0.7f, 1.05f, dist));
                col.a = alpha * Mathf.Lerp(0.25f, 0.55f, mottled);
                tex.SetPixel(x, y, col);
            }

            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
            Cache[key] = sprite;
            return sprite;
        }
    }
}
