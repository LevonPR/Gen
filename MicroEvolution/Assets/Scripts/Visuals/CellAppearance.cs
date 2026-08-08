using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Premium translucent microbe look: rim light, organelles, cilia/flagella, membrane tint.
    /// </summary>
    public class CellAppearance : MonoBehaviour
    {
        SpriteRenderer _body;
        SpriteRenderer _rim;
        SpriteRenderer _glow;
        SpriteRenderer _nucleus;
        SpriteRenderer _organelle;
        Transform _ciliaRoot;
        Transform _spikesRoot;
        Transform _flagellaRoot;
        Vector3 _bodyBase;
        Vector3 _nucleusBase;
        Vector3 _glowBase;
        float _pulse;
        bool _boosting;
        Color _membrane = new Color(0.35f, 0.8f, 1f, 0.7f);
        Faction _faction;

        public Color MembraneColor => _membrane;

        public void Build(Faction faction, float radius, Color bodyColor)
        {
            _faction = faction;
            _membrane = bodyColor;

            _glow = CreateChild("Glow", Vector3.zero, radius * 1.75f,
                ProceduralSprites.Circle($"glow-{faction}", new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.2f), 128), 4);
            _glowBase = _glow.transform.localScale;

            _rim = CreateChild("Rim", Vector3.zero, radius * 1.08f,
                ProceduralSprites.Ring($"rim-{faction}", new Color(
                    Mathf.Min(1f, bodyColor.r * 1.25f),
                    Mathf.Min(1f, bodyColor.g * 1.25f),
                    Mathf.Min(1f, bodyColor.b * 1.25f), 0.85f), 128, 0.14f), 6);

            _body = CreateChild("Body", Vector3.zero, radius,
                ProceduralSprites.Circle($"body-{faction}", bodyColor, 128), 5);
            _bodyBase = _body.transform.localScale;

            var nucleusColor = faction == Faction.Predator
                ? new Color(0.95f, 0.25f, 0.35f, 0.95f)
                : new Color(0.45f, 0.95f, 0.4f, 0.95f);

            _nucleus = CreateChild("Nucleus", new Vector3(-radius * 0.12f, radius * 0.08f, 0f), radius * 0.42f,
                ProceduralSprites.Circle($"nucleus-{faction}", nucleusColor, 96), 8);
            _nucleusBase = _nucleus.transform.localScale;

            _organelle = CreateChild("Organelle", new Vector3(radius * 0.28f, -radius * 0.15f, 0f), radius * 0.18f,
                ProceduralSprites.Circle("organelle-purple", new Color(0.75f, 0.4f, 1f, 0.9f), 48), 9);

            CreateChild("Vesicle", new Vector3(radius * 0.05f, -radius * 0.32f, 0f), radius * 0.12f,
                ProceduralSprites.Circle("vesicle-cyan", new Color(0.35f, 0.85f, 1f, 0.85f), 32), 9);

            BuildCilia(radius);
            BuildFlagella(radius);
            BuildSpikes(radius, faction == Faction.Predator);
            _spikesRoot.gameObject.SetActive(faction == Faction.Predator);
            _flagellaRoot.gameObject.SetActive(true);
        }

        void BuildCilia(float radius)
        {
            _ciliaRoot = new GameObject("Cilia").transform;
            _ciliaRoot.SetParent(transform, false);
            for (var i = 0; i < 14; i++)
            {
                var angle = (i / 14f) * Mathf.PI * 2f;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var cilia = CreateChild($"Cilium{i}", dir * radius * 0.82f, radius * 0.28f,
                    ProceduralSprites.Circle("cilia", new Color(0.7f, 0.95f, 1f, 0.4f), 24), 3);
                cilia.transform.localScale = new Vector3(radius * 0.06f, radius * 0.42f, 1f);
                cilia.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            }
        }

        void BuildFlagella(float radius)
        {
            _flagellaRoot = new GameObject("Flagella").transform;
            _flagellaRoot.SetParent(transform, false);
            for (var i = 0; i < 3; i++)
            {
                var fang = new GameObject($"Flagellum{i}");
                fang.transform.SetParent(_flagellaRoot, false);
                fang.transform.localRotation = Quaternion.Euler(0f, 0f, 160f + i * 20f);
                var sr = fang.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Spike("flagellum", new Color(_membrane.r, _membrane.g, _membrane.b, 0.75f));
                sr.sortingOrder = 2;
                fang.transform.localScale = new Vector3(radius * 0.55f, radius * 1.8f, 1f);
                fang.transform.localPosition = Vector3.down * radius * 0.2f;
            }
        }

        void BuildSpikes(float radius, bool predatorStyle)
        {
            _spikesRoot = new GameObject("Spikes").transform;
            _spikesRoot.SetParent(transform, false);
            var count = predatorStyle ? 10 : 6;
            for (var i = 0; i < count; i++)
            {
                var angle = i * (360f / count);
                var spike = new GameObject($"Spike{i}");
                spike.transform.SetParent(_spikesRoot, false);
                spike.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                var sr = spike.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Spike("spike",
                    predatorStyle ? new Color(0.3f, 0.15f, 0.2f, 0.95f) : new Color(0.85f, 0.95f, 1f, 0.9f));
                sr.sortingOrder = 7;
                spike.transform.localScale = Vector3.one * (radius * (predatorStyle ? 1.15f : 0.95f));
            }
        }

        public void SetMembraneColor(Color color)
        {
            _membrane = color;
            if (_body != null) _body.color = color;
            if (_glow != null) _glow.color = new Color(color.r, color.g, color.b, _boosting ? 0.5f : 0.2f);
            if (_rim != null)
                _rim.color = new Color(Mathf.Min(1f, color.r * 1.3f), Mathf.Min(1f, color.g * 1.3f), Mathf.Min(1f, color.b * 1.3f), 0.85f);
        }

        public void SetSpikesVisible(bool visible)
        {
            if (_spikesRoot != null) _spikesRoot.gameObject.SetActive(visible || _faction == Faction.Predator);
        }

        public void SetFlagellaVisible(bool visible)
        {
            if (_flagellaRoot != null) _flagellaRoot.gameObject.SetActive(visible);
        }

        public void SetBoostVisual(bool boosting)
        {
            _boosting = boosting;
            if (_glow == null) return;
            var c = _glow.color;
            c.a = boosting ? 0.5f : 0.2f;
            _glow.color = c;
            _glow.transform.localScale = _glowBase * (boosting ? 1.25f : 1f);
        }

        void Update()
        {
            _pulse += Time.deltaTime * 2.4f;
            if (_body != null)
                _body.transform.localScale = _bodyBase * (1f + Mathf.Sin(_pulse) * 0.035f);
            if (_nucleus != null)
                _nucleus.transform.localScale = _nucleusBase * (1f + Mathf.Sin(_pulse * 1.5f) * 0.1f);
            if (_ciliaRoot != null)
                _ciliaRoot.Rotate(0f, 0f, 28f * Time.deltaTime);
            if (_flagellaRoot != null)
                _flagellaRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(_pulse * 3f) * 8f);
        }

        SpriteRenderer CreateChild(string name, Vector3 localPos, float worldRadius, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            go.transform.localScale = Vector3.one * worldRadius;
            return sr;
        }
    }
}
