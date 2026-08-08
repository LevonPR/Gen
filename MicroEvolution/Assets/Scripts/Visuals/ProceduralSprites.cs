using System.Collections.Generic;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    public static class ProceduralSprites
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Circle(string key, Color color, int size = 64, float softEdge = 0.12f)
        {
            return SoftEllipse(key, color, size, 1f, 1f, softEdge, 0.35f);
        }

        public static Sprite SoftEllipse(string key, Color color, int size, float aspectX, float aspectY, float softEdge = 0.14f, float gloss = 0.4f)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var nx = (x - center) / (center * aspectX);
                    var ny = (y - center) / (center * aspectY);
                    var dist = Mathf.Sqrt(nx * nx + ny * ny);

                    float alpha;
                    if (dist <= 1f - softEdge) alpha = 1f;
                    else if (dist >= 1f) alpha = 0f;
                    else alpha = 1f - (dist - (1f - softEdge)) / softEdge;

                    // Gelatinous shading: darker rim, brighter core, specular highlight
                    var rim = Mathf.SmoothStep(0.55f, 1.1f, dist);
                    var core = 1.25f - rim * 0.7f;
                    var hx = nx + 0.28f;
                    var hy = ny + 0.32f;
                    var highlight = Mathf.Exp(-(hx * hx + hy * hy) * 6f) * gloss;

                    var shade = Mathf.Clamp01(core + highlight);
                    var c = color * shade;
                    // Slight subsurface-ish lift toward white in core
                    c.r = Mathf.Clamp01(c.r + highlight * 0.35f);
                    c.g = Mathf.Clamp01(c.g + highlight * 0.35f);
                    c.b = Mathf.Clamp01(c.b + highlight * 0.4f);
                    c.a = color.a * alpha;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite BloomDisc(string key, Color color, int size = 96)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x - center) / center;
                var dy = (y - center) / center;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                var a = Mathf.Clamp01(1f - dist);
                a = a * a * (3f - 2f * a); // smooth
                a = Mathf.Pow(a, 1.6f) * color.a;
                var c = color;
                c.a = a;
                tex.SetPixel(x, y, c);
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
                    // rim brighten
                    c *= 1f + outer * 0.35f;
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
                    var edge = 1f - Mathf.Abs(nx - 0.5f) / Mathf.Max(0.001f, halfWidth);
                    var c = color;
                    c *= 0.7f + edge * 0.5f;
                    c.a = inside ? color.a * (0.35f + 0.65f * ny) : 0f;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.1f), size / 2f);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite Capsule(string key, Color color, int size = 96)
        {
            return SoftEllipse(key, color, size, 0.55f, 1f, 0.16f, 0.45f);
        }

        public static Sprite SpikyOrb(string key, Color color, int size = 96, int spikes = 10)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x - center) / center;
                var dy = (y - center) / center;
                var ang = Mathf.Atan2(dy, dx);
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                var spike = 0.72f + 0.28f * Mathf.Pow(Mathf.Abs(Mathf.Sin(ang * spikes * 0.5f)), 1.4f);
                var limit = spike;
                float alpha;
                if (dist <= limit * 0.82f) alpha = 1f;
                else if (dist >= limit) alpha = 0f;
                else alpha = 1f - (dist - limit * 0.82f) / (limit * 0.18f);
                var shade = Mathf.Lerp(1.2f, 0.55f, dist / Mathf.Max(0.01f, limit));
                var c = color * shade;
                c.a = color.a * Mathf.Clamp01(alpha);
                tex.SetPixel(x, y, c);
            }

            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 2f);
            Cache[key] = sprite;
            return sprite;
        }
    }
}
