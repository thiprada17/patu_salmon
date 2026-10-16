using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlatuSalmon.Feedback
{
    public static class FeedbackUI
    {
        const int SS = 3;                       
        public const float PPU = 100f * SS;     

        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Func<string, Texture2D, Vector4, float, Sprite> SpriteFactory;

        public static void ClearCache() => cache.Clear();

        public static string T(string s) => string.IsNullOrEmpty(s) ? s : s.Replace('|', '​');

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static Color Hex(string hex, float a)
        {
            var c = Hex(hex);
            c.a = a;
            return c;
        }

        public static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }

        static bool Cached(string key, out Sprite s) => cache.TryGetValue(key, out s) && s != null;

        static Sprite Make(string key, Texture2D tex, Vector4 border, float ppu)
        {
            Sprite s = SpriteFactory != null
                ? SpriteFactory(key, tex, border, ppu)
                : Sprite.Create(tex, new UnityEngine.Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu, 0,
                    SpriteMeshType.FullRect, border);
            cache[key] = s;
            return s;
        }

        static float SdRoundBox(float px, float py, float h, float r)
        {
            float qx = Mathf.Abs(px) - (h - r);
            float qy = Mathf.Abs(py) - (h - r);
            float ox = Mathf.Max(qx, 0f), oy = Mathf.Max(qy, 0f);
            return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
        }

        static Texture2D NewTex(int size)
        {
            return new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        static Texture2D Gen(float sizeUnits, float half, float radius, float stroke, float soft)
        {
            int size = Mathf.CeilToInt(sizeUnits * SS);
            var tex = NewTex(size);
            var px = new Color32[size * size];
            float c = size * 0.5f;
            float h = half * SS, r = radius * SS, st = stroke * SS, sf = soft * SS;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = SdRoundBox(x + 0.5f - c, y + 0.5f - c, h, r);
                    float a;
                    if (sf > 0f)
                    {
                        float t = Mathf.Clamp01(0.5f - d / (2f * sf));
                        a = t * t * (3f - 2f * t);
                    }
                    else if (st > 0f) a = Mathf.Clamp01(0.5f - d) * Mathf.Clamp01(0.5f + d + st);
                    else a = Mathf.Clamp01(0.5f - d);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            return tex;
        }

        public static Sprite Rounded(int radius, float stroke = 0f)
        {
            string key = "rounded_" + radius + "_" + stroke;
            if (Cached(key, out var s)) return s;
            float size = radius * 2 + 6;
            var tex = Gen(size, size * 0.5f, radius, stroke, 0f);
            float b = (radius + 1) * SS;
            return Make(key, tex, new Vector4(b, b, b, b), PPU);
        }

        public static Sprite Circle(int radius, float stroke = 0f)
        {
            string key = "circle_" + radius + "_" + stroke;
            if (Cached(key, out var s)) return s;
            var tex = Gen(radius * 2, radius, radius, stroke, 0f);
            return Make(key, tex, Vector4.zero, PPU);
        }

        public static Sprite Soft(int radius, int blur)
        {
            string key = "shadow_" + radius + "_" + blur;
            if (Cached(key, out var s)) return s;
            float size = 2 * (radius + blur) + 6;
            var tex = Gen(size, size * 0.5f - blur, radius, 0f, blur);
            float b = (radius + blur + 1) * SS;
            return Make(key, tex, new Vector4(b, b, b, b), PPU);
        }

        public static Sprite Icon(string kind)
        {
            string key = "icon_" + kind;
            if (Cached(key, out var cached)) return cached;

            float vb = 24f, th = 2.6f;
            var lines = new List<Vector2[]>();
            switch (kind)
            {
                case "left": lines.Add(P(15, 5, 8, 12, 15, 19)); break;
                case "right": lines.Add(P(9, 5, 16, 12, 9, 19)); break;
                case "close": lines.Add(P(6, 6, 18, 18)); lines.Add(P(18, 6, 6, 18)); break;
                case "check": th = 3.4f; lines.Add(P(5, 12.5f, 9.5f, 17, 19, 7.5f)); break;
                case "enter": lines.Add(P(19, 5, 19, 12, 16, 15, 6, 15)); lines.Add(P(10, 11, 6, 15, 10, 19)); break;
                case "mail":
                case "mailheart":
                    vb = 64f;
                    lines.Add(P(4, 14, 60, 14, 60, 54, 4, 54, 4, 14));
                    lines.Add(P(6, 17, 32, 37, 58, 17));
                    if (kind == "mailheart")
                        lines.Add(P(32, 50, 24, 43, 22, 39, 24, 35.5f, 28, 35, 32, 39, 36, 35, 40, 35.5f, 42, 39, 40, 43, 32, 50));
                    break;
            }

            int size = 96;
            float scale = size / vb;
            var tex = NewTex(size);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2((x + 0.5f) / scale, vb - (y + 0.5f) / scale);
                    float d = float.MaxValue;
                    foreach (var line in lines)
                        for (int i = 0; i < line.Length - 1; i++)
                            d = Mathf.Min(d, SegDist(p, line[i], line[i + 1]));
                    float a = Mathf.Clamp01((th * 0.5f - d) * scale + 0.5f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            return Make(key, tex, Vector4.zero, 100f);
        }

        static Vector2[] P(params float[] v)
        {
            var a = new Vector2[v.Length / 2];
            for (int i = 0; i < a.Length; i++) a[i] = new Vector2(v[2 * i], v[2 * i + 1]);
            return a;
        }

        static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        public static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            int ui = LayerMask.NameToLayer("UI");
            go.layer = ui >= 0 ? ui : 0;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform Stretch(RectTransform rt, float l = 0, float t = 0, float r = 0, float b = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        public static Image NewImage(string name, Transform parent, Sprite sprite, Color color, bool sliced = true, bool raycast = false)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null) img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            return img;
        }

        public static TextMeshProUGUI NewText(string name, Transform parent, string text, TMP_FontAsset font, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.TopLeft, FontStyles style = FontStyles.Normal)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = T(text);
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.richText = true;
            t.raycastTarget = false;
            return t;
        }

        public static LayoutElement LE(Component c, float prefW = -1, float prefH = -1, float flexW = -1, float flexH = -1,
            float minW = -1, float minH = -1, bool ignore = false)
        {
            var le = c.GetComponent<LayoutElement>();
            if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = prefW;
            le.preferredHeight = prefH;
            le.flexibleWidth = flexW;
            le.flexibleHeight = flexH;
            le.minWidth = minW;
            le.minHeight = minH;
            le.ignoreLayout = ignore;
            return le;
        }

        public static TG Bg<TG>(TG g, float l = 0, float t = 0, float r = 0, float b = 0) where TG : Graphic
        {
            LE(g, ignore: true);
            Stretch(g.rectTransform, l, t, r, b);
            return g;
        }

        public static VerticalLayoutGroup VLG(Component c, float spacing = 0, RectOffset pad = null, TextAnchor align = TextAnchor.UpperLeft)
        {
            var g = c.gameObject.AddComponent<VerticalLayoutGroup>();
            g.spacing = spacing;
            g.padding = pad ?? new RectOffset();
            g.childAlignment = align;
            g.childControlWidth = true;
            g.childControlHeight = true;
            g.childForceExpandWidth = true;
            g.childForceExpandHeight = false;
            return g;
        }

        public static HorizontalLayoutGroup HLG(Component c, float spacing = 0, RectOffset pad = null, TextAnchor align = TextAnchor.UpperLeft)
        {
            var g = c.gameObject.AddComponent<HorizontalLayoutGroup>();
            g.spacing = spacing;
            g.padding = pad ?? new RectOffset();
            g.childAlignment = align;
            g.childControlWidth = true;
            g.childControlHeight = true;
            g.childForceExpandWidth = false;
            g.childForceExpandHeight = false;
            return g;
        }

        public static RectTransform Spacer(Transform parent, float h)
        {
            var rt = NewRect("Space", parent);
            LE(rt, prefH: h, minH: h);
            return rt;
        }

        public static GameObject EnsureEventSystem()
        {
#if UNITY_2023_1_OR_NEWER
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return null;
#else
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return null;
#endif
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
            return es;
        }

        public static float EaseOut(float t) { t = Mathf.Clamp01(t); return 1f - Mathf.Pow(1f - t, 3f); }

        public static float EaseInOut(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        public static float EaseOutBack(float t)
        {
            t = Mathf.Clamp01(t);
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
