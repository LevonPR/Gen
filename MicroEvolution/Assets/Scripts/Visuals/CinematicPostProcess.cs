using MicroEvolution.Mobile;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Built-in pipeline bloom + soft edge DoF + color grade via OnRenderImage.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CinematicPostProcess : MonoBehaviour
    {
        [Range(0f, 2f)] public float BloomIntensity = 0.9f;
        [Range(0f, 1.5f)] public float Threshold = 0.5f;
        [Range(0f, 1f)] public float SoftDoF = 0.4f;
        public Color GradeTint = new Color(0.88f, 0.96f, 1.08f, 1f);

        Material _mat;
        RenderTexture _bright;
        RenderTexture _blurA;
        RenderTexture _blurB;

        void OnEnable()
        {
            if (MobileSettings.IsMobileRuntime)
            {
                BloomIntensity = 0.65f;
                SoftDoF = 0.25f;
            }

            EnsureMaterial();
        }

        void OnDisable() => ReleaseTemps();

        void EnsureMaterial()
        {
            if (_mat != null) return;
            var shader = Shader.Find("Hidden/MicroEvolution/BloomDoF");
            if (shader != null) _mat = new Material(shader);
        }

        void ReleaseTemps()
        {
            if (_bright != null) _bright.Release();
            if (_blurA != null) _blurA.Release();
            if (_blurB != null) _blurB.Release();
            _bright = _blurA = _blurB = null;
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            EnsureMaterial();
            if (_mat == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            var w = Mathf.Max(8, src.width / 2);
            var h = Mathf.Max(8, src.height / 2);
            if (_bright == null || _bright.width != w || _bright.height != h)
            {
                ReleaseTemps();
                _bright = new RenderTexture(w, h, 0);
                _blurA = new RenderTexture(w, h, 0);
                _blurB = new RenderTexture(w, h, 0);
            }

            _mat.SetFloat("_Threshold", Threshold);
            _mat.SetFloat("_Intensity", BloomIntensity);
            _mat.SetFloat("_SoftDoF", SoftDoF);
            _mat.SetColor("_Tint", GradeTint);

            Graphics.Blit(src, _bright, _mat, 0);

            _mat.SetVector("_Direction", new Vector2(1f, 0f));
            Graphics.Blit(_bright, _blurA, _mat, 1);
            _mat.SetVector("_Direction", new Vector2(0f, 1f));
            Graphics.Blit(_blurA, _blurB, _mat, 1);
            _mat.SetVector("_Direction", new Vector2(1f, 0f));
            Graphics.Blit(_blurB, _blurA, _mat, 1);
            _mat.SetVector("_Direction", new Vector2(0f, 1f));
            Graphics.Blit(_blurA, _blurB, _mat, 1);

            _mat.SetTexture("_BloomTex", _blurB);
            Graphics.Blit(src, dest, _mat, 2);
        }

        public void SetBiomeGrade(Color tint, float bloom)
        {
            GradeTint = tint;
            BloomIntensity = bloom;
        }
    }
}
