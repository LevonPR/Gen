using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Player-centered bioluminescent halo that softly lights nearby microbes/food.
    /// </summary>
    public class BiolumLight : MonoBehaviour
    {
        SpriteRenderer _halo;
        SpriteRenderer _halo2;
        Transform _follow;
        float _pulse;

        public static BiolumLight Attach(Transform follow, Color color)
        {
            var go = new GameObject("BiolumLight");
            var light = go.AddComponent<BiolumLight>();
            light.Init(follow, color);
            return light;
        }

        void Init(Transform follow, Color color)
        {
            _follow = follow;
            _halo = CreateHalo("Halo", color, 8f, 0.22f, 12);
            _halo2 = CreateHalo("HaloInner", Color.Lerp(color, Color.white, 0.4f), 3.5f, 0.35f, 13);
        }

        SpriteRenderer CreateHalo(string name, Color color, float scale, float alpha, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.BloomDisc($"biolum-{name}", new Color(color.r, color.g, color.b, alpha), 128);
            sr.sortingOrder = order;
            go.transform.localScale = Vector3.one * scale;
            return sr;
        }

        void LateUpdate()
        {
            if (_follow == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = _follow.position;
            _pulse += Time.deltaTime * 2f;
            var boost = GameState.Instance != null && !GameState.Instance.PlayerDead ? 1f : 0.4f;
            if (_halo != null)
                _halo.transform.localScale = Vector3.one * (8f + Mathf.Sin(_pulse) * 0.4f) * boost;
            if (_halo2 != null)
                _halo2.transform.localScale = Vector3.one * (3.5f + Mathf.Sin(_pulse * 1.4f) * 0.25f) * boost;

            // Tint from membrane
            if (GameState.Instance != null && _halo != null)
            {
                var c = GameState.Instance.MembraneColor;
                _halo.color = new Color(c.r, c.g, c.b, 0.2f * boost);
                if (_halo2 != null) _halo2.color = new Color(c.r, c.g, c.b, 0.32f * boost);
            }
        }
    }
}
