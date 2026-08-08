using MicroEvolution.InputSystem;
using MicroEvolution.Mobile;
using UnityEngine;

namespace MicroEvolution.UI
{
    public class TouchControls : MonoBehaviour
    {
        public bool Visible = true;

        Rect _stickArea;
        Rect _stickKnob;
        Vector2 _stickCenter;
        bool _dragging;
        int _fingerId = -1;

        public void Configure()
        {
            Visible = MobileSettings.IsMobileRuntime || Application.isMobilePlatform ||
                      SystemInfo.deviceType == DeviceType.Handheld;
#if UNITY_EDITOR
            // Show in editor for preview when Game view aspect is phone-like, or always on for Android testing.
            Visible = true;
#endif
            _stickCenter = new Vector2(120f, Screen.height - 140f);
        }

        void Update()
        {
            if (!Visible || GameInput.Instance == null) return;
            HandleStick();
        }

        void HandleStick()
        {
            _stickArea = new Rect(_stickCenter.x - 70f, _stickCenter.y - 70f, 140f, 140f);

            for (var i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                var pos = t.position;
                pos.y = Screen.height - pos.y; // GUI space later; for logic use screen y up... keep y as Input
                var guiPos = new Vector2(t.position.x, Screen.height - t.position.y);

                if (t.phase == TouchPhase.Began && _stickArea.Contains(guiPos))
                {
                    _dragging = true;
                    _fingerId = t.fingerId;
                }

                if (_dragging && t.fingerId == _fingerId)
                {
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        _dragging = false;
                        _fingerId = -1;
                        GameInput.Instance.SetTouchMove(Vector2.zero);
                    }
                    else
                    {
                        var delta = guiPos - _stickCenter;
                        // Convert GUI delta (y down) to world move (y up)
                        var move = new Vector2(delta.x, -delta.y) / 70f;
                        GameInput.Instance.SetTouchMove(Vector2.ClampMagnitude(move, 1f));
                        _stickKnob = new Rect(_stickCenter.x + delta.x - 24f, _stickCenter.y + delta.y - 24f, 48f, 48f);
                    }
                }
            }

            // Mouse simulation for editor
            if (Input.touchCount == 0)
            {
                var mouseGui = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                if (Input.GetMouseButtonDown(0) && _stickArea.Contains(mouseGui) && mouseGui.x < Screen.width * 0.4f)
                    _dragging = true;

                if (_dragging && Input.GetMouseButton(0))
                {
                    var delta = mouseGui - _stickCenter;
                    var move = new Vector2(delta.x, -delta.y) / 70f;
                    GameInput.Instance.SetTouchMove(Vector2.ClampMagnitude(move, 1f));
                    _stickKnob = new Rect(_stickCenter.x + delta.x - 24f, _stickCenter.y + delta.y - 24f, 48f, 48f);
                }
                else if (_dragging && Input.GetMouseButtonUp(0))
                {
                    _dragging = false;
                    GameInput.Instance.SetTouchMove(Vector2.zero);
                }
            }

            if (!_dragging)
                _stickKnob = new Rect(_stickCenter.x - 24f, _stickCenter.y - 24f, 48f, 48f);
        }

        public void DrawGUI(GUIStyle panel, GUIStyle button)
        {
            if (!Visible) return;
            _stickCenter = new Vector2(120f, Screen.height - 140f);
            _stickArea = new Rect(_stickCenter.x - 70f, _stickCenter.y - 70f, 140f, 140f);
            GUI.Box(_stickArea, GUIContent.none, panel);
            GUI.Box(_stickKnob, GUIContent.none, button);

            var boostRect = new Rect(Screen.width - 170f, Screen.height - 170f, 70f, 70f);
            var chemRect = new Rect(Screen.width - 90f, Screen.height - 200f, 70f, 70f);
            var evoRect = new Rect(Screen.width - 90f, Screen.height - 110f, 70f, 54f);
            var pauseRect = new Rect(Screen.width - 70f, 170f, 50f, 40f);

            var boost = GUI.RepeatButton(boostRect, "Q", button);
            GameInput.Instance?.SetTouchBoost(boost);
            if (GUI.Button(chemRect, "E", button)) GameInput.Instance?.PressTouchChem();
            if (GUI.Button(evoRect, "EVO", button)) GameInput.Instance?.PressTouchEvolve();
            if (GUI.Button(pauseRect, "II", button)) GameInput.Instance?.PressTouchPause();
        }
    }
}
