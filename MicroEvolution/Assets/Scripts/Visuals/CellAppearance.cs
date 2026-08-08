using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Hybrid mesh + sprite microbe: translucent rim-lit mesh body when SoftCell shader exists.
    /// </summary>
    public class CellAppearance : MonoBehaviour
    {
        MeshRenderer _meshBody;
        MeshFilter _meshFilter;
        Material _cellMat;
        SpriteRenderer _rim;
        SpriteRenderer _bloom;
        SpriteRenderer _bloom2;
        SpriteRenderer _nucleus;
        SpriteRenderer _organelle;
        SpriteRenderer _fallbackBody;
        Transform _ciliaRoot;
        Transform _spikesRoot;
        Transform _flagellaRoot;
        Transform _segmentsRoot;
        Transform _partsRoot;
        Vector3 _bodyBase = Vector3.one;
        Vector3 _nucleusBase;
        Vector3 _bloomBase;
        float _pulse;
        bool _boosting;
        Color _membrane = new Color(0.35f, 0.8f, 1f, 0.7f);
        Faction _faction;
        MicrobeStyle _style;
        float _radius = 0.85f;

        static Mesh _sphereMesh;
        static Mesh _capsuleMesh;
        static Shader _softShader;

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
            _radius = radius;

            _bloom2 = CreateSprite("BloomOuter", Vector3.zero, radius * 2.7f,
                ProceduralSprites.BloomDisc($"bloom2-{style}", new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.25f), 128), 2);
            _bloom = CreateSprite("Bloom", Vector3.zero, radius * 2f,
                ProceduralSprites.BloomDisc($"bloom-{style}", new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.38f), 128), 3);
            _bloomBase = _bloom.transform.localScale;

            BuildMeshOrSpriteBody(radius, bodyColor, style);

            _rim = CreateSprite("Rim", Vector3.zero, radius * 1.14f,
                ProceduralSprites.Ring($"rim-{style}", new Color(
                    Mathf.Min(1f, bodyColor.r * 1.4f),
                    Mathf.Min(1f, bodyColor.g * 1.4f),
                    Mathf.Min(1f, bodyColor.b * 1.4f), 0.92f), 128, 0.11f), 7);

            var nucleusColor = faction == Faction.Predator
                ? new Color(0.95f, 0.25f, 0.35f, 0.95f)
                : new Color(0.45f, 0.95f, 0.4f, 0.95f);
            _nucleus = CreateSprite("Nucleus", new Vector3(-radius * 0.1f, radius * 0.08f, -0.02f), radius * 0.38f,
                ProceduralSprites.SoftEllipse($"nucleus-{style}", nucleusColor, 96, 1f, 1f, 0.18f, 0.55f), 9);
            _nucleusBase = _nucleus.transform.localScale;

            _organelle = CreateSprite("Organelle", new Vector3(radius * 0.26f, -radius * 0.14f, -0.02f), radius * 0.16f,
                ProceduralSprites.Circle("organelle-purple", new Color(0.75f, 0.4f, 1f, 0.9f), 48), 10);

            CreateSprite("Vesicle", new Vector3(radius * 0.02f, -radius * 0.3f, -0.02f), radius * 0.1f,
                ProceduralSprites.Circle("vesicle-cyan", new Color(0.35f, 0.85f, 1f, 0.85f), 32), 10);

            _partsRoot = new GameObject("Parts").transform;
            _partsRoot.SetParent(transform, false);

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
        }

        void BuildMeshOrSpriteBody(float radius, Color bodyColor, MicrobeStyle style)
        {
            if (_softShader == null) _softShader = Shader.Find("MicroEvolution/SoftCell");
            var useMesh = _softShader != null && style != MicrobeStyle.SpikyOrb;

            if (!useMesh)
            {
                Sprite bodySprite;
                Vector3 bodyScale;
                if (style == MicrobeStyle.SpikyOrb)
                {
                    bodySprite = ProceduralSprites.SpikyOrb($"spiky-{_faction}", bodyColor, 128, 12);
                    bodyScale = Vector3.one * radius * 1.15f;
                }
                else if (style == MicrobeStyle.RodBacteria)
                {
                    bodySprite = ProceduralSprites.Capsule($"rod-{_faction}", bodyColor, 128);
                    bodyScale = new Vector3(radius * 0.7f, radius * 1.35f, 1f);
                }
                else
                {
                    bodySprite = ProceduralSprites.SoftEllipse($"body-{_faction}-{style}", bodyColor, 128, 0.9f, 1.05f, 0.15f, 0.5f);
                    bodyScale = Vector3.one * radius;
                }

                _fallbackBody = CreateSprite("Body", Vector3.zero, 1f, bodySprite, 5);
                _fallbackBody.transform.localScale = bodyScale;
                _bodyBase = bodyScale;
                return;
            }

            if (_sphereMesh == null) _sphereMesh = MeshFactory.UnitSphere(28, 18);
            if (_capsuleMesh == null) _capsuleMesh = MeshFactory.Capsule(22, 14);

            var bodyGo = new GameObject("MeshBody");
            bodyGo.transform.SetParent(transform, false);
            _meshFilter = bodyGo.AddComponent<MeshFilter>();
            _meshBody = bodyGo.AddComponent<MeshRenderer>();
            _meshFilter.sharedMesh = style == MicrobeStyle.RodBacteria ? _capsuleMesh : _sphereMesh;
            _cellMat = new Material(_softShader);
            _cellMat.SetColor("_Color", bodyColor);
            _cellMat.SetColor("_RimColor", Color.Lerp(bodyColor, Color.white, 0.55f));
            _meshBody.sharedMaterial = _cellMat;
            _meshBody.sortingOrder = 5;

            var scale = style == MicrobeStyle.RodBacteria
                ? new Vector3(radius * 1.5f, radius * 2.1f, radius * 1.2f)
                : Vector3.one * (radius * 2f);
            bodyGo.transform.localScale = scale;
            _bodyBase = scale;
        }

        void BuildSegments(float radius, Color color)
        {
            _segmentsRoot = new GameObject("Segments").transform;
            _segmentsRoot.SetParent(transform, false);
            for (var i = 1; i <= 4; i++)
            {
                var seg = CreateSprite($"Seg{i}", new Vector3(0f, -radius * 0.55f * i, 0f), 1f,
                    ProceduralSprites.SoftEllipse($"seg-{i}", color * (1f - i * 0.06f), 96, 0.8f, 1f, 0.16f, 0.4f), 4);
                seg.transform.localScale = new Vector3(radius * (0.75f - i * 0.06f), radius * (0.9f - i * 0.05f), 1f);
            }
        }

        void BuildCilia(float radius)
        {
            _ciliaRoot = new GameObject("Cilia").transform;
            _ciliaRoot.SetParent(_partsRoot != null ? _partsRoot : transform, false);
            for (var i = 0; i < 16; i++)
            {
                var angle = (i / 16f) * Mathf.PI * 2f;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var cilia = CreateSprite($"Cilium{i}", dir * radius * 0.84f, radius * 0.28f,
                    ProceduralSprites.BloomDisc("cilia", new Color(0.75f, 0.95f, 1f, 0.55f), 24), 3);
                cilia.transform.SetParent(_ciliaRoot, true);
                cilia.transform.localScale = new Vector3(radius * 0.07f, radius * 0.48f, 1f);
                cilia.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            }
        }

        void BuildFlagella(float radius)
        {
            _flagellaRoot = new GameObject("Flagella").transform;
            _flagellaRoot.SetParent(_partsRoot != null ? _partsRoot : transform, false);
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
            _spikesRoot.SetParent(_partsRoot != null ? _partsRoot : transform, false);
            for (var i = 0; i < 10; i++)
            {
                var angle = i * 36f;
                var spike = new GameObject($"Spike{i}");
                spike.transform.SetParent(_spikesRoot, false);
                spike.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                var sr = spike.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Spike("spike", new Color(0.9f, 0.95f, 1f, 0.85f));
                sr.sortingOrder = 8;
                spike.transform.localScale = Vector3.one * (radius * 1.05f);
            }

            _spikesRoot.gameObject.SetActive(enabled);
        }

        public void RefreshAttachedParts(GameState state)
        {
            if (state == null) return;
            SetSpikesVisible(state.HasSpikes);
            SetFlagellaVisible(state.HasFlagella || state.HasOscillator);
            // Eyes: small bright dots
            var eyes = transform.Find("Parts/Eyes");
            if (state.HasEyes && eyes == null)
            {
                var e = CreateSprite("Eyes", new Vector3(_radius * 0.2f, _radius * 0.22f, -0.03f), _radius * 0.12f,
                    ProceduralSprites.Circle("eye", new Color(1f, 1f, 0.7f, 0.95f), 32), 11);
                e.transform.SetParent(_partsRoot != null ? _partsRoot : transform, true);
                e.gameObject.name = "Eyes";
            }
            else if (!state.HasEyes && eyes != null) Destroy(eyes.gameObject);

            // Jaws: front spikes
            var jaws = transform.Find("Parts/Jaws");
            if (state.HasJaws && jaws == null)
            {
                var j = new GameObject("Jaws");
                j.transform.SetParent(_partsRoot != null ? _partsRoot : transform, false);
                for (var i = 0; i < 3; i++)
                {
                    var t = CreateSprite($"Jaw{i}", new Vector3((i - 1) * _radius * 0.12f, _radius * 0.7f, 0f), _radius * 0.35f,
                        ProceduralSprites.Spike("jaw", new Color(0.95f, 0.85f, 0.7f, 0.9f)), 8);
                    t.transform.SetParent(j.transform, true);
                    t.transform.localRotation = Quaternion.Euler(0f, 0f, -20f + i * 20f);
                }
            }
            else if (!state.HasJaws && jaws != null) Destroy(jaws.gameObject);
        }

        public void SetMembraneColor(Color color)
        {
            _membrane = color;
            if (_fallbackBody != null) _fallbackBody.color = color;
            if (_cellMat != null)
            {
                _cellMat.SetColor("_Color", color);
                _cellMat.SetColor("_RimColor", Color.Lerp(color, Color.white, 0.55f));
            }

            if (_bloom != null) _bloom.color = new Color(color.r, color.g, color.b, _boosting ? 0.55f : 0.38f);
            if (_bloom2 != null) _bloom2.color = new Color(color.r, color.g, color.b, 0.22f);
            if (_rim != null)
                _rim.color = new Color(Mathf.Min(1f, color.r * 1.4f), Mathf.Min(1f, color.g * 1.4f), Mathf.Min(1f, color.b * 1.4f), 0.92f);
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
                c.a = boosting ? 0.65f : 0.38f;
                _bloom.color = c;
                _bloom.transform.localScale = _bloomBase * (boosting ? 1.35f : 1f);
            }

            if (_cellMat != null)
                _cellMat.SetFloat("_FresnelBoost", boosting ? 2.1f : 1.4f);
        }

        void Update()
        {
            _pulse += Time.deltaTime * 2.4f;
            var breathe = new Vector3(
                1f + Mathf.Sin(_pulse) * 0.04f,
                1f + Mathf.Sin(_pulse + 0.8f) * 0.03f,
                1f);

            if (_meshBody != null)
                _meshBody.transform.localScale = Vector3.Scale(_bodyBase, breathe);
            if (_fallbackBody != null)
                _fallbackBody.transform.localScale = Vector3.Scale(_bodyBase, breathe);
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
                    t.localPosition = new Vector3(wave, t.localPosition.y, t.localPosition.z);
                }
            }
        }

        SpriteRenderer CreateSprite(string name, Vector3 localPos, float worldRadius, Sprite sprite, int order)
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
