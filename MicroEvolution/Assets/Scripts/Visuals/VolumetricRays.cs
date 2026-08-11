using MicroEvolution.Core;
using MicroEvolution.Mobile;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    public class VolumetricRays : MonoBehaviour
    {
        Transform _root;
        Material _mat;
        float _pulse;

        public static VolumetricRays Create(Transform parent)
        {
            var go = new GameObject("VolumetricRays");
            go.transform.SetParent(parent, false);
            var v = go.AddComponent<VolumetricRays>();
            v.Build();
            return v;
        }

        void Build()
        {
            var shader = Shader.Find("MicroEvolution/GodRay");
            _mat = shader != null ? new Material(shader) : null;
            _root = new GameObject("Rays").transform;
            _root.SetParent(transform, false);

            var count = MobileSettings.IsMobileRuntime ? 5 : 9;
            for (var i = 0; i < count; i++)
            {
                var ray = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Object.Destroy(ray.GetComponent<Collider>());
                ray.name = $"Ray{i}";
                ray.transform.SetParent(_root, false);
                ray.transform.position = new Vector3(Random.Range(-35f, 35f), Random.Range(8f, 28f), 1.5f);
                ray.transform.localScale = new Vector3(Random.Range(2.5f, 5.5f), Random.Range(28f, 48f), 1f);
                ray.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-16f, 16f));

                var mr = ray.GetComponent<MeshRenderer>();
                if (_mat != null)
                {
                    var m = new Material(_mat);
                    var a = Random.Range(0.08f, 0.16f);
                    m.SetColor("_Color", new Color(0.55f, 0.88f, 1f, a));
                    m.SetFloat("_Power", Random.Range(1.2f, 2.4f));
                    mr.sharedMaterial = m;
                }
                else
                {
                    mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                    mr.sharedMaterial.color = new Color(0.55f, 0.88f, 1f, 0.1f);
                }

                mr.sortingOrder = -24;
            }

            // Foam motes near surface
            var foamCount = MobileSettings.IsMobileRuntime ? 18 : 36;
            for (var i = 0; i < foamCount; i++)
            {
                var foam = new GameObject($"Foam{i}");
                foam.transform.SetParent(transform, false);
                foam.transform.position = new Vector3(
                    Random.Range(-GameConfig.WorldRadius, GameConfig.WorldRadius),
                    Random.Range(-GameConfig.WorldRadius, GameConfig.WorldRadius),
                    0f);
                var sr = foam.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.BloomDisc("foam", new Color(0.85f, 0.95f, 1f, Random.Range(0.15f, 0.4f)), 32);
                sr.sortingOrder = -4;
                foam.transform.localScale = Vector3.one * Random.Range(0.2f, 0.7f);
                foam.AddComponent<FoamDrift>().Speed = Random.Range(0.05f, 0.2f);
            }
        }

        void Update()
        {
            _pulse += Time.deltaTime * 0.12f;
            if (_root != null)
            {
                _root.localPosition = new Vector3(Mathf.Sin(_pulse) * 3f, Mathf.Cos(_pulse * 0.6f) * 1.5f, 0f);
                foreach (Transform ray in _root)
                {
                    var c = ray.GetComponent<MeshRenderer>()?.sharedMaterial;
                    if (c != null && c.HasProperty("_Color"))
                    {
                        var col = c.GetColor("_Color");
                        col.a = Mathf.Clamp01(col.a * (0.92f + Mathf.Sin(_pulse * 3f + ray.GetInstanceID()) * 0.08f));
                        // keep base alpha range by not compounding — rewrite from name hash
                    }

                    ray.Rotate(0f, 0f, Mathf.Sin(_pulse * 2f + ray.localPosition.x) * 0.02f);
                }
            }
        }

        class FoamDrift : MonoBehaviour
        {
            public float Speed = 0.1f;
            Vector2 _dir;
            float _t;

            void Start()
            {
                _dir = Random.insideUnitCircle.normalized;
                _t = Random.value * 10f;
            }

            void Update()
            {
                _t += Time.deltaTime;
                transform.position += (Vector3)(_dir * Speed * Time.deltaTime);
                transform.localScale = Vector3.one * (0.35f + Mathf.Sin(_t * 2f) * 0.08f);
                if (transform.position.magnitude > GameConfig.WorldRadius)
                {
                    transform.position = (Vector3)(-transform.position.normalized * GameConfig.WorldRadius * 0.9f);
                    _dir = Random.insideUnitCircle.normalized;
                }
            }
        }
    }
}
