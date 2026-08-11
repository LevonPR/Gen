using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    public class VfxBurst : MonoBehaviour
    {
        public static void Spawn(Vector3 position, Color color, float size = 0.6f)
        {
            if (SaveSystem.LoadMeta().reducedParticles) return;

            var go = new GameObject("VfxBurst");
            go.transform.position = position;
            var vfx = go.AddComponent<VfxBurst>();
            vfx.Init(color, size);
        }

        SpriteRenderer _sr;
        float _life = 0.35f;
        float _age;
        float _size;

        void Init(Color color, float size)
        {
            _size = size;
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = ProceduralSprites.Circle("burst", color, 32);
            _sr.sortingOrder = 30;
            transform.localScale = Vector3.one * size * 0.4f;
        }

        void Update()
        {
            _age += Time.unscaledDeltaTime;
            var t = _age / _life;
            transform.localScale = Vector3.one * Mathf.Lerp(_size * 0.4f, _size * 1.6f, t);
            if (_sr != null)
            {
                var c = _sr.color;
                c.a = 1f - t;
                _sr.color = c;
            }

            if (t >= 1f) Destroy(gameObject);
        }
    }

    public class FloatingText : MonoBehaviour
    {
        public static void Spawn(Vector3 worldPos, string text, Color color)
        {
            var go = new GameObject("FloatText");
            var ft = go.AddComponent<FloatingText>();
            ft.Init(worldPos, text, color);
        }

        string _text;
        Color _color;
        Vector3 _pos;
        float _age;

        void Init(Vector3 worldPos, string text, Color color)
        {
            _pos = worldPos;
            _text = text;
            _color = color;
        }

        void Update()
        {
            _age += Time.unscaledDeltaTime;
            _pos += Vector3.up * (1.2f * Time.unscaledDeltaTime);
            if (_age > 0.9f) Destroy(gameObject);
        }

        void OnGUI()
        {
            if (Camera.main == null) return;
            var screen = Camera.main.WorldToScreenPoint(_pos);
            if (screen.z < 0f) return;
            var y = Screen.height - screen.y;
            var prev = GUI.color;
            GUI.color = new Color(_color.r, _color.g, _color.b, 1f - _age / 0.9f);
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(screen.x - 40, y - 10, 80, 24), _text, style);
            GUI.color = prev;
        }
    }

    public class CameraImpulse : MonoBehaviour
    {
        public static CameraImpulse Instance { get; private set; }
        Vector3 _punch;
        Transform _cam;

        void Awake()
        {
            Instance = this;
            _cam = transform;
        }

        public void Punch(float amount = 0.15f)
        {
            _punch = Random.insideUnitCircle * amount;
        }

        void LateUpdate()
        {
            if (_punch.sqrMagnitude < 0.0001f) return;
            _cam.position += _punch;
            _punch = Vector3.Lerp(_punch, Vector3.zero, 12f * Time.unscaledDeltaTime);
        }
    }
}
