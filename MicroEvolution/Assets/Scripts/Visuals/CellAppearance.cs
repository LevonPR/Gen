using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    public class CellAppearance : MonoBehaviour
    {
        SpriteRenderer _body;
        SpriteRenderer _nucleus;
        SpriteRenderer _glow;
        Transform _ciliaRoot;
        Transform _spikesRoot;
        Vector3 _bodyBaseScale;
        Vector3 _nucleusBaseScale;
        float _pulse;

        public void Build(Faction faction, float radius, Color bodyColor)
        {
            _glow = CreateChild("Glow", Vector3.zero, radius * 1.55f,
                ProceduralSprites.Circle($"glow-{faction}", new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.22f), 96), 5);

            _body = CreateChild("Body", Vector3.zero, radius,
                ProceduralSprites.Circle($"body-{faction}", bodyColor, 96), 6);
            _bodyBaseScale = _body.transform.localScale;

            var nucleusColor = faction == Faction.Predator
                ? new Color(0.95f, 0.35f, 0.25f, 0.95f)
                : new Color(1f, 0.55f, 0.2f, 0.95f);

            _nucleus = CreateChild("Nucleus", new Vector3(-radius * 0.15f, radius * 0.1f, 0f), radius * 0.38f,
                ProceduralSprites.Circle($"nucleus-{faction}", nucleusColor, 64), 7);
            _nucleusBaseScale = _nucleus.transform.localScale;

            CreateChild("VesicleA", new Vector3(radius * 0.28f, -radius * 0.18f, 0f), radius * 0.16f,
                ProceduralSprites.Circle("vesicle-green", new Color(0.35f, 0.9f, 0.55f, 0.85f), 32), 8);

            CreateChild("VesicleB", new Vector3(radius * 0.05f, -radius * 0.32f, 0f), radius * 0.12f,
                ProceduralSprites.Circle("vesicle-blue", new Color(0.35f, 0.7f, 1f, 0.8f), 32), 8);

            _ciliaRoot = new GameObject("Cilia").transform;
            _ciliaRoot.SetParent(transform, false);

            for (var i = 0; i < 10; i++)
            {
                var angle = (i / 10f) * Mathf.PI * 2f + 0.3f;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var cilia = CreateChild($"Cilium{i}", dir * radius * 0.75f, radius * 0.35f,
                    ProceduralSprites.Circle("cilia", new Color(0.55f, 0.85f, 1f, 0.45f), 24), 4);
                cilia.transform.localScale = new Vector3(radius * 0.08f, radius * 0.35f, 1f);
                cilia.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            }

            _spikesRoot = new GameObject("Spikes").transform;
            _spikesRoot.SetParent(transform, false);
            _spikesRoot.gameObject.SetActive(false);

            for (var i = 0; i < 6; i++)
            {
                var angle = i * 60f;
                var spike = new GameObject($"Spike{i}");
                spike.transform.SetParent(_spikesRoot, false);
                spike.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                var sr = spike.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Spike("spike", new Color(0.85f, 0.9f, 1f, 0.9f));
                sr.sortingOrder = 9;
                spike.transform.localScale = Vector3.one * (radius * 0.9f);
            }

            if (faction == Faction.Predator)
            {
                for (var i = 0; i < 8; i++)
                {
                    var angle = i * 45f;
                    var spike = new GameObject($"PredSpike{i}");
                    spike.transform.SetParent(transform, false);
                    spike.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                    var sr = spike.AddComponent<SpriteRenderer>();
                    sr.sprite = ProceduralSprites.Spike("pred-spike", new Color(0.25f, 0.2f, 0.28f, 0.95f));
                    sr.sortingOrder = 3;
                    spike.transform.localScale = Vector3.one * (radius * 1.1f);
                }
            }
        }

        public void SetSpikesVisible(bool visible)
        {
            if (_spikesRoot != null) _spikesRoot.gameObject.SetActive(visible);
        }

        public void SetBoostVisual(bool boosting)
        {
            if (_glow == null) return;
            var c = _glow.color;
            c.a = boosting ? 0.45f : 0.22f;
            _glow.color = c;
        }

        void Update()
        {
            _pulse += Time.deltaTime * 2.2f;
            if (_body != null)
                _body.transform.localScale = _bodyBaseScale * (1f + Mathf.Sin(_pulse) * 0.03f);

            if (_nucleus != null)
                _nucleus.transform.localScale = _nucleusBaseScale * (1f + Mathf.Sin(_pulse * 1.4f) * 0.08f);

            if (_ciliaRoot != null)
                _ciliaRoot.Rotate(0f, 0f, 25f * Time.deltaTime);
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
