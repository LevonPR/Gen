using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Gelatinous microbe renderer with bloom layers and silhouette styles from concept art.
    /// </summary>
    public class CellAppearance : MonoBehaviour
    {
        SpriteRenderer _body;
        SpriteRenderer _rim;
        SpriteRenderer _bloom;
        SpriteRenderer _bloom2;
        SpriteRenderer _nucleus;
        SpriteRenderer _organelle;
        Transform _ciliaRoot;
        Transform _spikesRoot;
        Transform _flagellaRoot;
        Transform _segmentsRoot;
        Vector3 _bodyBase;
        Vector3 _nucleusBase;
        Vector3 _bloomBase;
        float _pulse;
        bool _boosting;
        Color _membrane = new Color(0.35f, 0.8f, 1f, 0.7f);
        Faction _faction;
        MicrobeStyle _style;

        public Color MembraneColor => _membrane;
        public MicrobeStyle Style => _style;

        public void Build(Faction faction, float radius, Color bodyColor)
        {
            var style = faction == Faction.Player ? MicrobeStyle.Eukaryote
                : faction == Faction.Ally ? MicrobeStyle.AllyProbe
                : faction == Faction.Predator ? (Random.value > 0.45f ? MicrobeStyle.SpikyOrb : MicrobeStyle.Segmented)
                : (Random.value > 0.5f ? MicrobeStyle.RodBacteria : MicrobeStyle.Eukaryote);
            Build(faction, radius, bodyColor, style);
        }

        public void Build(Faction faction, float radius, Color bodyColor, MicrobeStyle style)
        {
            _faction = faction;
            _style = style;
            _membrane = bodyColor;

            // Wide additive bloom (fake HDR glow)
            _bloom2 = CreateChild("BloomOuter", Vector3.zero, radius * 2.6f,
                ProceduralSprites.BloomDisc($"bloom2-{style}", new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.22f), 128), 2);
            _bloom = CreateChild("Bloom", Vector3.zero, radius * 1.9f,
                ProceduralSprites.BloomDisc($"bloom-{style}", new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.35f), 128), 3);
            _bloomBase = _bloom.transform.localScale;

            Sprite bodySprite;
            Vector3 bodyScale;
            switch (style)
            {
                case MicrobeStyle.SpikyOrb:
                    bodySprite = ProceduralSprites.SpikyOrb($"spiky-{faction}", bodyColor, 128, 12);
                    bodyScale = Vector3.one * radius * 1.15f;
                    break;
                case MicrobeStyle.RodBacteria:
                    bodySprite = ProceduralSprites.Capsule($"rod-{faction}", bodyColor, 128);
                    bodyScale = new Vector3(radius * 0.7f, radius * 1.35f, 1f);
                    break;
                case MicrobeStyle.Segmented:
                    bodySprite = ProceduralSprites.SoftEllipse($"seg-head-{faction}", bodyColor, 128, 0.75f, 1f, 0.15f, 0.5f);
                    bodyScale = new Vector3(radius * 0.85f, radius * 1.05f, 1f);
                    break;
                default:
                    bodySprite = ProceduralSprites.SoftEllipse($"body-{faction}-{style}", bodyColor, 128, 0.9f, 1.05f, 0.15f, 0.5f);
                    bodyScale = Vector3.one * radius;
                    break;
            }

            _rim = CreateChild("Rim", Vector3.zero, radius * 1.12f,
                ProceduralSprites.Ring($"rim-{style}", new Color(
                    Mathf.Min(1f, bodyColor.r * 1.35f),
                    Mathf.Min(1f, bodyColor.g * 1.35f),
                    Mathf.Min(1f, bodyColor.b * 1.35f), 0.9f), 128, 0.12f), 6);

            _body = CreateChild("Body", Vector3.zero, 1f, bodySprite, 5);
            _body.transform.localScale = bodyScale;
            _bodyBase = bodyScale;

            var nucleusColor = faction == Faction.Predator
                ? new Color(0.95f, 0.25f, 0.35f, 0.95f)
                : new Color(0.45f, 0.95f, 0.4f, 0.95f);

            _nucleus = CreateChild("Nucleus", new Vector3(-radius * 0.1f, radius * 0.08f, 0f), radius * 0.4f,
                ProceduralSprites.SoftEllipse($"nucleus-{style}", nucleusColor, 96, 1f, 1f, 0.18f, 0.55f), 8);
            _nucleusBase = _nucleus.transform.localScale;

            _organelle = CreateChild("Organelle", new Vector3(radius * 0.26f, -radius * 0.14f, 0f), radius * 0.17f,
                ProceduralSprites.Circle("organelle-purple", new Color(0.75f, 0.4f, 1f, 0.9f), 48), 9);

            CreateChild("Vesicle", new Vector3(radius * 0.02f, -radius * 0.3f, 0f), radius * 0.11f,
                ProceduralSprites.Circle("vesicle-cyan", new Color(0.35f, 0.85f, 1f, 0.85f), 32), 9);

            if (style == MicrobeStyle.Segmented)
                BuildSegments(radius, bodyColor);

            if (style == MicrobeStyle.Eukaryote || style == MicrobeStyle.AllyProbe || faction == Faction.Player)
            {
                BuildCilia(radius);
                BuildFlagella(radius);
            }

            BuildSpikes(radius, style == MicrobeStyle.SpikyOrb || faction == Faction.Predator);
            if (_spikesRoot != null)
                _spikesRoot.gameObject.SetActive(style == MicrobeStyle.SpikyOrb || faction == Faction.Predator);

            if (style == MicrobeStyle.RodBacteria && _rim != null)
                _rim.transform.localScale = new Vector3(radius * 0.75f, radius * 1.4f, 1f);
        }

        void BuildSegments(float radius, Color color)
        {
            _segmentsRoot = new GameObject("Segments").transform;
            _segmentsRoot.SetParent(transform, false);
            for (var i = 1; i <= 4; i++)
            {
                var seg = CreateChild($"Seg{i}", new Vector3(0f, -radius * 0.55f * i, 0f), radius * (0.85f - i * 0.08f),
                    ProceduralSprites.SoftEllipse($"seg-{i}", color * (1f - i * 0.06f), 96, 0.8f, 1f, 0.16f, 0.4f), 4);
                seg.transform.localScale = new Vector3(radius * (0.75f - i * 0.06f), radius * (0.9f - i * 0.05f), 1f);
            }
        }

        void BuildCilia(float radius)
        {
            _ciliaRoot = new GameObject("Cilia").transform;
            _ciliaRoot.SetParent(transform, false);
            for (var i = 0; i < 16; i++)
            {
                var angle = (i / 16f) * Mathf.PI * 2f;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var cilia = CreateChild($"Cilium{i}", dir * radius * 0.84f, radius * 0.28f,
                    ProceduralSprites.BloomDisc("cilia", new Color(0.75f, 0.95f, 1f, 0.55f), 24), 3);
                cilia.transform.localScale = new Vector3(radius * 0.07f, radius * 0.48f, 1f);
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
                fang.transform.localRotation = Quaternion.Euler(0f, 0f, 155f + i * 22f);
                var sr = fang.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Spike("flagellum", new Color(_membrane.r, _membrane.g, _membrane.b, 0.8f));
                sr.sortingOrder = 2;
                fang.transform.localScale = new Vector3(radius * 0.5f, radius * 2.1f, 1f);
                fang.transform.localPosition = Vector3.down * radius * 0.15f;
            }
        }

        void BuildSpikes(float radius, bool enabled)
        {
            _spikesRoot = new GameObject("Spikes").transform;
            _spikesRoot.SetParent(transform, false);
            for (var i = 0; i < 10; i++)
            {
                var angle = i * 36f;
                var spike = new GameObject($"Spike{i}");
                spike.transform.SetParent(_spikesRoot, false);
                spike.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                var sr = spike.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Spike("spike", new Color(0.9f, 0.95f, 1f, 0.85f));
                sr.sortingOrder = 7;
                spike.transform.localScale = Vector3.one * (radius * 1.05f);
            }

            _spikesRoot.gameObject.SetActive(enabled);
        }

        public void SetMembraneColor(Color color)
        {
            _membrane = color;
            if (_body != null) _body.color = color;
            if (_bloom != null) _bloom.color = new Color(color.r, color.g, color.b, _boosting ? 0.55f : 0.35f);
            if (_bloom2 != null) _bloom2.color = new Color(color.r, color.g, color.b, 0.2f);
            if (_rim != null)
                _rim.color = new Color(Mathf.Min(1f, color.r * 1.35f), Mathf.Min(1f, color.g * 1.35f), Mathf.Min(1f, color.b * 1.35f), 0.9f);
        }

        public void SetSpikesVisible(bool visible)
        {
            if (_spikesRoot != null)
                _spikesRoot.gameObject.SetActive(visible || _style == MicrobeStyle.SpikyOrb || _faction == Faction.Predator);
        }

        public void SetFlagellaVisible(bool visible)
        {
            if (_flagellaRoot != null) _flagellaRoot.gameObject.SetActive(visible);
        }

        public void SetBoostVisual(bool boosting)
        {
            _boosting = boosting;
            if (_bloom != null)
            {
                var c = _bloom.color;
                c.a = boosting ? 0.6f : 0.35f;
                _bloom.color = c;
                _bloom.transform.localScale = _bloomBase * (boosting ? 1.3f : 1f);
            }

            if (_bloom2 != null)
                _bloom2.transform.localScale = Vector3.one * (_bodyBase.magnitude * (boosting ? 3.1f : 2.6f));
        }

        void Update()
        {
            _pulse += Time.deltaTime * 2.4f;
            if (_body != null)
                _body.transform.localScale = Vector3.Scale(_bodyBase, new Vector3(
                    1f + Mathf.Sin(_pulse) * 0.04f,
                    1f + Mathf.Sin(_pulse + 0.8f) * 0.03f,
                    1f));
            if (_nucleus != null)
                _nucleus.transform.localScale = _nucleusBase * (1f + Mathf.Sin(_pulse * 1.5f) * 0.1f);
            if (_ciliaRoot != null)
                _ciliaRoot.Rotate(0f, 0f, 30f * Time.deltaTime);
            if (_flagellaRoot != null)
                _flagellaRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(_pulse * 3.2f) * 10f);
            if (_segmentsRoot != null)
            {
                for (var i = 0; i < _segmentsRoot.childCount; i++)
                {
                    var t = _segmentsRoot.GetChild(i);
                    var wave = Mathf.Sin(_pulse * 2.5f + i * 0.7f) * 0.08f;
                    t.localPosition = new Vector3(wave, t.localPosition.y, 0f);
                }
            }
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
