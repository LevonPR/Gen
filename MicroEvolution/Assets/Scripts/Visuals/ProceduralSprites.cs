using System.Collections.Generic;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    public static class ProceduralSprites
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Circle(string key, Color color, int size = 64, float softEdge = 0.12f)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = (size - 1) * 0.5f;
            var radius = center;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                    float alpha;
                    if (dist <= 1f - softEdge) alpha = 1f;
                    else if (dist >= 1f) alpha = 0f;
                    else alpha = 1f - (dist - (1f - softEdge)) / softEdge;

                    var shade = Mathf.Lerp(1.15f, 0.65f, dist);
                    var c = color * shade;
                    c.a = color.a * alpha;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite Ring(string key, Color color, int size = 64, float thickness = 0.18f)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = (size - 1) * 0.5f;
            var radius = center;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                    var outer = Mathf.Clamp01(1f - Mathf.Abs(dist - (1f - thickness)) / thickness);
                    var c = color;
                    c.a *= outer * (dist < 1f ? 1f : 0f);
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite Spike(string key, Color color, int size = 32)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var nx = x / (float)(size - 1);
                    var ny = y / (float)(size - 1);
                    var halfWidth = Mathf.Lerp(0.5f, 0.02f, ny);
                    var inside = Mathf.Abs(nx - 0.5f) <= halfWidth;
                    var c = color;
                    c.a = inside ? color.a * (0.4f + 0.6f * ny) : 0f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.1f), size / 2f);
            Cache[key] = sprite;
            return sprite;
        }
    }
}
