using UnityEngine;

namespace MicroEvolution.InputSystem
{
    /// <summary>
    /// Unified input for keyboard/mouse and on-screen touch controls.
    /// </summary>
    public class GameInput : MonoBehaviour
    {
        public static GameInput Instance { get; private set; }

        public Vector2 Move { get; private set; }
        public bool BoostHeld { get; private set; }
        public bool ChemPressed { get; private set; }
        public bool PausePressed { get; private set; }
        public bool EvolvePanelPressed { get; private set; }

        Vector2 _touchMove;
        bool _touchBoost;
        bool _touchChem;
        bool _touchEvolve;
        bool _touchPause;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Update()
        {
            var keyMove = Vector2.zero;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow)) keyMove.x -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow)) keyMove.x += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow)) keyMove.y -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow)) keyMove.y += 1f;

            if (UnityEngine.Input.GetMouseButton(1) && Camera.main != null && Core.GameState.Instance != null &&
                Core.GameState.Instance.PlayerTransform != null)
            {
                var world = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                world.z = 0f;
                var to = (Vector2)(world - Core.GameState.Instance.PlayerTransform.position);
                if (to.sqrMagnitude > 0.01f) keyMove = to.normalized;
            }

            Move = _touchMove.sqrMagnitude > 0.01f ? Vector2.ClampMagnitude(_touchMove, 1f) : Vector2.ClampMagnitude(keyMove, 1f);
            BoostHeld = _touchBoost || UnityEngine.Input.GetKey(KeyCode.Q);
            ChemPressed = _touchChem || UnityEngine.Input.GetKeyDown(KeyCode.E) || UnityEngine.Input.GetKeyDown(KeyCode.F);
            PausePressed = _touchPause || UnityEngine.Input.GetKeyDown(KeyCode.Escape) || UnityEngine.Input.GetKeyDown(KeyCode.P);
            EvolvePanelPressed = _touchEvolve || UnityEngine.Input.GetKeyDown(KeyCode.Tab);

            // Edge-trigger touch buttons reset after one frame.
            _touchChem = false;
            _touchEvolve = false;
            _touchPause = false;
        }

        public void SetTouchMove(Vector2 value) => _touchMove = value;
        public void SetTouchBoost(bool value) => _touchBoost = value;
        public void PressTouchChem() => _touchChem = true;
        public void PressTouchEvolve() => _touchEvolve = true;
        public void PressTouchPause() => _touchPause = true;
    }
}
