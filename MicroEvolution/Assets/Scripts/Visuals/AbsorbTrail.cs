using UnityEngine;

namespace MicroEvolution.Visuals
{
    public class AbsorbTrail : MonoBehaviour
    {
        public static void Burst(Vector3 from, Vector3 to, Color color)
        {
            var go = new GameObject("AbsorbTrail");
            var trail = go.AddComponent<AbsorbTrail>();
            trail.Init(from, to, color);
        }

        Vector3 _from;
        Vector3 _to;
        SpriteRenderer[] _dots;
        float _age;

        void Init(Vector3 from, Vector3 to, Color color)
        {
            _from = from;
            _to = to;
            _dots = new SpriteRenderer[6];
            for (var i = 0; i < _dots.Length; i++)
            {
                var d = new GameObject($"Dot{i}");
                d.transform.SetParent(transform, false);
                var sr = d.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.BloomDisc("absorb-dot", color, 24);
                sr.sortingOrder = 28;
                d.transform.localScale = Vector3.one * 0.25f;
                _dots[i] = sr;
            }
        }

        void Update()
        {
            _age += Time.deltaTime;
            var t = _age / 0.45f;
            for (var i = 0; i < _dots.Length; i++)
            {
                var u = Mathf.Clamp01(t - i * 0.05f);
                var p = Vector3.Lerp(_from, _to, u);
                p += (Vector3)(Random.insideUnitCircle * 0.02f);
                _dots[i].transform.position = p;
                var c = _dots[i].color;
                c.a = (1f - u) * 0.8f;
                _dots[i].color = c;
                _dots[i].transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 0.05f, u);
            }

            if (t > 1.2f) Destroy(gameObject);
        }
    }
}
