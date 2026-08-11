using UnityEngine;
using UnityEngine.UI;

namespace MicroEvolution.UI
{
    public static class UiTheme
    {
        public static readonly Color BgPanel = new Color(0.02f, 0.08f, 0.12f, 0.72f);
        public static readonly Color BgPanelSolid = new Color(0.03f, 0.1f, 0.14f, 0.92f);
        public static readonly Color AccentCyan = new Color(0.35f, 0.9f, 1f, 1f);
        public static readonly Color AccentGreen = new Color(0.35f, 0.95f, 0.55f, 1f);
        public static readonly Color AccentDna = new Color(0.45f, 1f, 0.7f, 1f);
        public static readonly Color AccentOrange = new Color(1f, 0.55f, 0.25f, 1f);
        public static readonly Color AccentRed = new Color(1f, 0.35f, 0.4f, 1f);
        public static readonly Color AccentYellow = new Color(1f, 0.85f, 0.3f, 1f);
        public static readonly Color TextPrimary = new Color(0.88f, 0.96f, 1f, 1f);
        public static readonly Color TextDim = new Color(0.65f, 0.8f, 0.9f, 0.9f);
        public static readonly Color Joystick = new Color(0.15f, 0.35f, 0.45f, 0.45f);
        public static readonly Color HexSlot = new Color(0.08f, 0.2f, 0.28f, 0.85f);

        public static Font Font
        {
            get
            {
                var names = Font.GetOSInstalledFontNames();
                foreach (var prefer in new[] { "Segoe UI", "Ubuntu", "DejaVu Sans", "Arial" })
                {
                    foreach (var n in names)
                        if (n.IndexOf(prefer, System.StringComparison.OrdinalIgnoreCase) >= 0)
                            return Font.CreateDynamicFontFromOSFont(n, 16);
                }

                return Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        public static Sprite WhiteSprite
        {
            get
            {
                if (_white != null) return _white;
                var t = Texture2D.whiteTexture;
                _white = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f);
                return _white;
            }
        }

        static Sprite _white;

        public static Text MakeText(Transform parent, string content, int size, Color color, FontStyle style = FontStyle.Normal, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.fontStyle = style;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        public static Image MakePanel(Transform parent, string name, Color color, bool raycast = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = WhiteSprite;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static RectTransform Stretch(Component c)
        {
            var rt = c.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static void SetAnchored(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        public static Button MakeButton(Transform parent, string name, Color color)
        {
            var img = MakePanel(parent, name, color, true);
            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = color * 1.2f;
            colors.pressedColor = color * 0.8f;
            btn.colors = colors;
            return btn;
        }

        public static Image MakeBarFill(Transform parent, Color color)
        {
            var img = MakePanel(parent, "Fill", color);
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 1f;
            return img;
        }
    }
}
