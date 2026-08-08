using MicroEvolution.Core;
using MicroEvolution.Mobile;
using UnityEngine;

namespace MicroEvolution.Visuals
{
    /// <summary>
    /// Lightweight cinematic grade: vignette, warmth pulse, hurt flash — no URP required.
    /// </summary>
    public class CameraEffects : MonoBehaviour
    {
        public static CameraEffects Instance { get; private set; }

        Texture2D _vignetteTex;
        float _hurtFlash;
        float _pulse;
        Color _baseBg = new Color(0.015f, 0.06f, 0.1f, 1f);
        Camera _cam;

        void Awake()
        {
            Instance = this;
            _cam = GetComponent<Camera>();
            if (_cam != null) _baseBg = _cam.backgroundColor;
            BuildVignette();
        }

        void BuildVignette()
        {
            const int size = 256;
            _vignetteTex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x - center) / center;
                var dy = (y - center) / center;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                var a = Mathf.SmoothStep(0.45f, 1.15f, dist) * 0.55f;
                _vignetteTex.SetPixel(x, y, new Color(0f, 0.02f, 0.04f, a));
            }

            _vignetteTex.Apply();
        }

        void OnEnable() => GameEvents.PlayerHurt += OnHurt;
        void OnDisable() => GameEvents.PlayerHurt -= OnHurt;
        void OnHurt(float _) => _hurtFlash = 0.45f;

        void Update()
        {
            _pulse += Time.unscaledDeltaTime * 0.25f;
            if (_hurtFlash > 0f) _hurtFlash -= Time.unscaledDeltaTime * 1.4f;

            if (_cam == null) return;
            var warm = 0.01f + Mathf.Sin(_pulse) * 0.008f;
            var bg = _baseBg;
            bg.r += warm + _hurtFlash * 0.12f;
            bg.b += 0.01f;
            _cam.backgroundColor = bg;

            var baseSize = MobileSettings.IsMobileRuntime ? GameConfig.CameraSizeMobile : GameConfig.CameraSize;
            _cam.orthographicSize = baseSize + Mathf.Sin(_pulse * 1.3f) * 0.12f + _hurtFlash * 0.35f;
        }

        void OnGUI()
        {
            var prev = GUI.color;
            // Vignette is handled by CinematicPostProcess soft-DoF when present.
            if (GetComponent<CinematicPostProcess>() == null && _vignetteTex != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _vignetteTex, ScaleMode.StretchToFill, true);
            }

            if (_hurtFlash > 0f)
            {
                GUI.color = new Color(0.8f, 0.1f, 0.15f, _hurtFlash * 0.35f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            }

            GUI.color = prev;
        }
    }
}
