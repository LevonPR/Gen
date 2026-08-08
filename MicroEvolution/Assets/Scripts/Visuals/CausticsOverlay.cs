using MicroEvolution.Core;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    public class CausticsOverlay : MonoBehaviour
    {
        public static CausticsOverlay Create(Transform parent)
        {
            var go = new GameObject("CausticsOverlay");
            go.transform.SetParent(parent, false);
            var c = go.AddComponent<CausticsOverlay>();
            c.Build();
            return c;
        }

        void Build()
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.Destroy(quad.GetComponent<Collider>());
            quad.name = "CausticsQuad";
            quad.transform.SetParent(transform, false);
            quad.transform.position = Vector3.zero;
            quad.transform.localScale = Vector3.one * (GameConfig.WorldRadius * 2.4f);
            // Face camera (XY plane already for Quad default facing +Z; rotate for ortho top-down style)
            quad.transform.rotation = Quaternion.identity;

            var shader = Shader.Find("MicroEvolution/Caustics");
            var mr = quad.GetComponent<MeshRenderer>();
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.SetColor("_Color", new Color(0.55f, 0.92f, 1f, 0.14f));
                mr.sharedMaterial = mat;
            }

            mr.sortingOrder = -15;
        }

        void LateUpdate()
        {
            if (GameState.Instance != null && GameState.Instance.PlayerTransform != null)
                transform.position = GameState.Instance.PlayerTransform.position * 0.15f;
        }
    }
}
