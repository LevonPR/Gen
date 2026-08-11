using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Lightweight bone-chain animation for cilia / flagella segments.
    /// </summary>
    public class AppendageChain : MonoBehaviour
    {
        public int Segments = 5;
        public float SegmentLength = 0.18f;
        public float WaveSpeed = 8f;
        public float WaveAmplitude = 18f;
        public float Phase;
        public Color Color = new Color(0.7f, 0.95f, 1f, 0.75f);
        public bool IsFlagellum;

        Transform[] _bones;
        float _t;

        public void Build()
        {
            _bones = new Transform[Segments];
            Transform parent = transform;
            for (var i = 0; i < Segments; i++)
            {
                var bone = new GameObject($"Bone{i}");
                bone.transform.SetParent(parent, false);
                bone.transform.localPosition = i == 0 ? Vector3.zero : Vector3.up * SegmentLength;
                var sr = bone.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.BloomDisc($"append-{i}", Color, 24);
                sr.sortingOrder = 3;
                var taper = Mathf.Lerp(0.14f, 0.05f, i / (float)Mathf.Max(1, Segments - 1));
                var len = IsFlagellum ? SegmentLength * 1.4f : SegmentLength;
                bone.transform.localScale = new Vector3(taper, len * 1.1f, 1f);
                _bones[i] = bone.transform;
                parent = bone.transform;
            }
        }

        void Update()
        {
            if (_bones == null) return;
            _t += Time.deltaTime * WaveSpeed;
            for (var i = 0; i < _bones.Length; i++)
            {
                var wave = Mathf.Sin(_t + Phase + i * 0.55f) * WaveAmplitude * (IsFlagellum ? 1.2f : 0.7f);
                // secondary noise
                wave += Mathf.Sin(_t * 1.7f + i) * WaveAmplitude * 0.25f;
                _bones[i].localRotation = Quaternion.Euler(0f, 0f, wave);
            }
        }
    }
}
