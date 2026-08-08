using UnityEngine;

namespace MicroEvolution.Visuals
{
    public class MitosisFx : MonoBehaviour
    {
        public static void Play(Vector3 position, Color color)
        {
            var go = new GameObject("MitosisFx");
            go.transform.position = position;
            var fx = go.AddComponent<MitosisFx>();
            fx.Init(color);
        }

        SpriteRenderer _a;
        SpriteRenderer _b;
        SpriteRenderer _bridge;
        float _age;

        void Init(Color color)
        {
            _a = Make(color, new Vector3(-0.2f, 0f, 0f));
            _b = Make(color, new Vector3(0.2f, 0f, 0f));
            _bridge = Make(new Color(color.r, color.g, color.b, 0.5f), Vector3.zero);
            _bridge.transform.localScale = new Vector3(0.8f, 0.25f, 1f);
            VfxBurst.Spawn(transform.position, color, 2.2f);
        }

        SpriteRenderer Make(Color color, Vector3 local)
        {
            var go = new GameObject("Cell");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = local;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.SoftEllipse("mito", color, 64, 0.9f, 1f, 0.18f, 0.5f);
            sr.sortingOrder = 25;
            go.transform.localScale = Vector3.one * 0.9f;
            return sr;
        }

        void Update()
        {
            _age += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_age / 1.4f);
            if (_a != null) _a.transform.localPosition = Vector3.Lerp(new Vector3(-0.15f, 0, 0), new Vector3(-1.1f, 0.2f, 0), t);
            if (_b != null) _b.transform.localPosition = Vector3.Lerp(new Vector3(0.15f, 0, 0), new Vector3(1.1f, -0.15f, 0), t);
            if (_bridge != null)
            {
                _bridge.transform.localScale = new Vector3(Mathf.Lerp(0.9f, 0.05f, t), 0.22f, 1f);
                var c = _bridge.color;
                c.a = (1f - t) * 0.5f;
                _bridge.color = c;
            }

            if (t >= 1f) Destroy(gameObject);
        }
    }
}
