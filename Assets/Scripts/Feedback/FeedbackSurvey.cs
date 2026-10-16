using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using static PlatuSalmon.Feedback.FeedbackUI;

namespace PlatuSalmon.Feedback
{

    [DisallowMultipleComponent]
    public class FeedbackSurvey : MonoBehaviour
    {
        [Header("Fonts (TMP Font Assets with Thai support)")]
        [Tooltip("Font for all text (e.g. Itim)")] public TMP_FontAsset bodyFont;
        [Tooltip("Optional font for headings and numbers. Leave empty to use Body Font.")] public TMP_FontAsset headingFont;

        [Header("Colors")]
        public FeedbackTheme theme = new FeedbackTheme();

        [Header("Texts  (all wording except questions and choices; Build UI keeps these)")]
        public FeedbackTexts texts = new FeedbackTexts();

        [Header("Icons  (optional: drag your own sprites here, then press Build UI)")]
        public FeedbackIcons customIcons = new FeedbackIcons();

        [Header("Content  (use '|' between Thai words to mark line-break points)")]
        public List<FeedbackSection> sections = new List<FeedbackSection>();
        public List<FeedbackQuestion> questions = new List<FeedbackQuestion>();

        [Header("Open / Submit")]
        [Tooltip("Show the survey as soon as the scene starts")] public bool openOnStart = true;
        public int sortingOrder = 100;
        [Tooltip("If set, answers are POSTed here as JSON (e.g. a FastAPI endpoint)")]
        public string submitUrl = "";

        [Serializable] public class StringEvent : UnityEvent<string> { }
        [Tooltip("Called when the player confirms sending, with the answers as JSON")] public StringEvent onSubmit = new StringEvent();
        [Tooltip("Called after the survey closes (sent or left)")] public UnityEvent onClosed = new UnityEvent();

        [Header("UI references  (filled in by Build UI, no need to drag)")]
        public RectTransform uiRoot;
        public RectTransform stack;
        public List<FeedbackCardView> cards = new List<FeedbackCardView>();
        public Button closeButton;
        public RectTransform pill;
        public TextMeshProUGUI pillText;
        public FeedbackModalView confirmModal = new FeedbackModalView();
        public FeedbackModalView thanksModal = new FeedbackModalView();
        public FeedbackModalView leaveModal = new FeedbackModalView();
        public Sprite iconNext;
        public Sprite iconSend;

        class Answer
        {
            public int scale = -1;
            public readonly List<string> sel = new List<string>();
            public string other = "";
            public string text = "";
        }

        readonly Dictionary<string, Answer> answers = new Dictionary<string, Answer>();
        int cur;
        bool fix;
        bool isOpen;
        bool opening;
        bool bound;
        int navId;
        int lastRemaining = -1;
        FeedbackModalView currentModal;

        int N => questions.Count;
        public TMP_FontAsset BodyFont => bodyFont != null ? bodyFont : TMP_Settings.defaultFontAsset;
        public TMP_FontAsset HeadFont => headingFont != null ? headingFont : BodyFont;
        public bool IsOpen => isOpen;

        public bool HasUI =>
            uiRoot != null && stack != null && pill != null && cards != null &&
            questions != null && cards.Count == questions.Count && cards.TrueForAll(c => c != null);

        void Reset()
        {
            sections = new List<FeedbackSection>();
            questions = new List<FeedbackQuestion>();
            FeedbackDefaults.Fill(sections, questions);
        }

        public void EnsureContent()
        {
            if (sections == null) sections = new List<FeedbackSection>();
            if (questions == null) questions = new List<FeedbackQuestion>();
            if (sections.Count == 0 || questions.Count == 0) FeedbackDefaults.Fill(sections, questions);
        }

        void Awake()
        {
            Init();
            if (opening || isOpen) return;
            if (openOnStart) Open();
            else gameObject.SetActive(false);
        }

        void Init()
        {
            if (bound) return;
            EnsureContent();
            if (!HasUI)
            {
                Debug.LogError("[FeedbackSurvey] UI references are missing. Check the UI section of this component in the Inspector.", this);
                return;
            }
            ConvertWrapMarks();
            KeepCardNumbersOnOneLine();
            EnsureEventSystem();
            Bind();
            bound = true;
        }

        void ConvertWrapMarks()
        {
            foreach (var t in GetComponentsInChildren<TMP_Text>(true))
                if (t.text != null && t.text.IndexOf('|') >= 0) t.text = T(t.text);
            foreach (var f in GetComponentsInChildren<TMP_InputField>(true))
                if (f.placeholder is TMP_Text ph && ph.text != null) ph.text = T(ph.text);
        }

        void KeepCardNumbersOnOneLine()
        {
            foreach (var c in cards)
            {
                var header = c.body != null ? c.body.Find("Content/Header") : null;
                if (header == null) continue;
                var number = header.Find("Number");
                var name = header.Find("Section Name");
                if (number != null) SetMinWidth(number.gameObject, 86f);
                if (name != null)
                {
                    var le = name.GetComponent<LayoutElement>();
                    if (le == null) le = name.gameObject.AddComponent<LayoutElement>();
                    le.preferredWidth = 0f;
                    le.flexibleWidth = 1f;
                }
            }
        }

        static void SetMinWidth(GameObject go, float w)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.minWidth = w;
        }


        public void Open()
        {
            if (isOpen || questions == null || questions.Count == 0) return;
            Init();
            if (!bound) return;
            ResetState();
            opening = true;
            gameObject.SetActive(true);
            opening = false;
            isOpen = true;
            RefreshAll(true);
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            StopAllCoroutines();
            HideModals();
            gameObject.SetActive(false);
            onClosed.Invoke();
        }

        void ResetState()
        {
            StopAllCoroutines();
            answers.Clear();
            cur = 0;
            fix = false;
            navId++;
            lastRemaining = -1;
            HideModals();
            foreach (var c in cards)
            {
                c.move = null;
                c.shake = null;
                c.body.anchoredPosition = Vector2.zero;
                c.body.localRotation = Quaternion.identity;
                if (c.input != null) c.input.SetTextWithoutNotify("");
                foreach (var o in c.options)
                    if (o.other != null) o.other.SetTextWithoutNotify("");
            }
        }

        void Bind()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var c = cards[i];
                c.questionIndex = i;
                var q = questions[i];
                Listen(c.back, Back);
                Listen(c.primary, Primary);
                foreach (var p in c.points)
                {
                    int v = p.value;
                    Listen(p.button, () => SetScale(q, v));
                }
                foreach (var o in c.options)
                {
                    var opt = q.options.Find(x => x.id == o.optionId);
                    if (opt == null) continue;
                    Listen(o.button, () => ToggleOption(q, opt));
                    if (o.other != null)
                    {
                        o.other.onValueChanged.RemoveAllListeners();
                        o.other.onValueChanged.AddListener(v => { A(q).other = v; OnAnswerChanged(); });
                    }
                }
                if (c.input != null)
                {
                    c.input.characterLimit = q.maxChars;
                    c.input.onValueChanged.RemoveAllListeners();
                    c.input.onValueChanged.AddListener(v => { A(q).text = v; OnAnswerChanged(); });
                }
            }
            Listen(closeButton, () => ShowModal(leaveModal));
            Listen(confirmModal?.cancel, HideModals);
            Listen(confirmModal?.ok, Submit);
            Listen(leaveModal?.cancel, HideModals);
            Listen(leaveModal?.ok, Close);
        }

        static void Listen(Button b, UnityAction action)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(action);
        }

        public void PreviewCard(int index)
        {
            if (!HasUI) return;
            answers.Clear();
            fix = false;
            cur = Mathf.Clamp(index, 0, N - 1);
            foreach (var c in cards)
            {
                c.body.anchoredPosition = Vector2.zero;
                c.body.localRotation = Quaternion.identity;
            }
            RefreshAll(true);
        }

        public void PreviewModal(int which)
        {
            var all = new[] { confirmModal, thanksModal, leaveModal };
            for (int k = 0; k < all.Length; k++)
            {
                var m = all[k];
                if (m == null || m.root == null) continue;
                m.root.SetActive(k == which);
                if (m.group != null) m.group.alpha = 1f;
                if (m.box != null)
                {
                    m.box.localScale = Vector3.one;
                    m.box.localRotation = Quaternion.identity;
                    m.box.anchoredPosition = Vector2.zero;
                }
                if (m.drainFill != null) m.drainFill.anchorMax = Vector2.one;
            }
        }

        Answer A(FeedbackQuestion q)
        {
            if (!answers.TryGetValue(q.id, out var a))
            {
                a = new Answer();
                answers[q.id] = a;
            }
            return a;
        }

        bool Done(int i) => questions[i].optional || Answered(i);

        bool Answered(int i)
        {
            var q = questions[i];
            var a = A(q);
            switch (q.type)
            {
                case QuestionType.Scale: return a.scale > 0;
                case QuestionType.Text: return !string.IsNullOrWhiteSpace(a.text);
                default:
                    int n = a.sel.Count;
                    if (n < Mathf.Max(1, q.min)) return false;
                    if (q.max > 0 && n > q.max) return false;
                    var other = q.options.Find(o => o.isOther);
                    if (other != null && a.sel.Contains(other.id) && string.IsNullOrWhiteSpace(a.other)) return false;
                    return true;
            }
        }

        List<int> Remaining()
        {
            var r = new List<int>();
            for (int i = 0; i < N; i++) if (!Done(i)) r.Add(i);
            return r;
        }

        void SetScale(FeedbackQuestion q, int v)
        {
            A(q).scale = v;
            OnAnswerChanged();
        }

        void ToggleOption(FeedbackQuestion q, FeedbackOption o)
        {
            var a = A(q);
            if (q.type == QuestionType.Single)
            {
                a.sel.Clear();
                a.sel.Add(o.id);
            }
            else if (a.sel.Contains(o.id)) a.sel.Remove(o.id);
            else if (o.exclusive)
            {
                a.sel.Clear();
                a.sel.Add(o.id);
            }
            else
            {
                a.sel.RemoveAll(id =>
                {
                    var f = q.options.Find(x => x.id == id);
                    return f != null && f.exclusive;
                });
                if (q.max > 0 && a.sel.Count >= q.max) return;
                a.sel.Add(o.id);
            }
            OnAnswerChanged();
        }

        void OnAnswerChanged()
        {
            if (isOpen) RefreshAll(false);
        }

        void Primary()
        {
            if (!isOpen || currentModal != null) return;
            var rem = Remaining();
            if (!fix)
            {
                if (cur < N - 1) { Move(cur + 1, "fwd"); return; }
                if (rem.Count == 0) { ShowModal(confirmModal); return; }
                fix = true;
                Move(rem[0], "jump");
                return;
            }
            if (rem.Count == 0) { ShowModal(confirmModal); return; }
            if (!Done(cur)) { Move(cur, "jump"); return; }
            int next = rem.FirstOrDefault(i => i > cur);
            if (next <= cur) next = rem[0];
            Move(next, "jump");
        }

        void Back()
        {
            if (!isOpen || currentModal != null || cur == 0) return;
            Move(cur - 1, "back");
        }

        int Depth(int i) => (i - cur + N) % N;

        void Move(int to, string kind)
        {
            int from = cur;
            cur = to;
            navId++;

            if (kind == "jump" && to == from)
            {
                RefreshAll(false);
                StartShake(cards[to]);
                return;
            }

            int outIdx = kind == "fwd" || kind == "jump" ? from : -1;
            int inIdx = kind == "back" || kind == "jump" ? to : -1;

            var rem = Remaining();
            ApplyOrder(outIdx, inIdx);
            foreach (var c in cards)
            {
                RefreshCard(c, rem);
                int i = c.questionIndex;
                bool animating = i == outIdx || i == inIdx;
                ApplyDepth(c, !animating);
                if (c.move != null) StopCoroutine(c.move);
                if (i == outIdx) c.move = StartCoroutine(OutRoutine(c));
                else if (i == inIdx) c.move = StartCoroutine(InRoutine(c, kind == "jump"));
                else c.move = StartCoroutine(TweenToRest(c, 0.55f));
            }
            StartCoroutine(MidOrder(navId, 0.38f));
            UpdatePill(rem.Count);
        }

        void ApplyOrder(int forceTop, int forceBottom)
        {
            var order = Enumerable.Range(0, N).OrderByDescending(Depth).ToList();
            for (int k = 0; k < order.Count; k++) cards[order[k]].Slot.SetSiblingIndex(k);
            if (forceTop >= 0) cards[forceTop].Slot.SetAsLastSibling();
            if (forceBottom >= 0) cards[forceBottom].Slot.SetAsFirstSibling();
        }

        IEnumerator MidOrder(int id, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (id == navId) ApplyOrder(-1, -1);
        }

        void GetRest(FeedbackCardView c, out Vector2 pos, out float rot)
        {
            if (Depth(c.questionIndex) == 0)
            {
                pos = Vector2.zero;
                rot = 0f;
                return;
            }
            pos = c.restOffset;
            rot = c.restRotation;
        }

        static void SetPose(FeedbackCardView c, Vector2 p, float r)
        {
            c.Slot.anchoredPosition = p;
            c.rot = r;
            c.Slot.localRotation = Quaternion.Euler(0, 0, r);
        }

        static IEnumerator Seg(FeedbackCardView c, Vector2 p0, float r0, Vector2 p1, float r1, float dur, Func<float, float> ease)
        {
            for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
            {
                float e = ease(t / dur);
                SetPose(c, Vector2.LerpUnclamped(p0, p1, e), Mathf.LerpUnclamped(r0, r1, e));
                yield return null;
            }
            SetPose(c, p1, r1);
        }

        IEnumerator TweenToRest(FeedbackCardView c, float dur)
        {
            Vector2 p1;
            float r1;
            GetRest(c, out p1, out r1);
            yield return Seg(c, c.Slot.anchoredPosition, c.rot, p1, r1, dur, EaseOut);
        }

        IEnumerator OutRoutine(FeedbackCardView c)
        {
            c.slotGroup.alpha = 1f;
            float h = c.body.rect.height;
            var low = new Vector2(-8, -h * 1.25f);
            yield return Seg(c, c.Slot.anchoredPosition, c.rot, low, 8f, 0.36f, EaseInOut);
            Vector2 p1;
            float r1;
            GetRest(c, out p1, out r1);
            yield return Seg(c, low, 8f, p1, r1, 0.44f, EaseOut);
            ApplyDepth(c, true);
        }

        IEnumerator InRoutine(FeedbackCardView c, bool shakeAfter)
        {
            c.slotGroup.alpha = 1f;
            float h = c.body.rect.height;
            var low = new Vector2(8, -h * 1.25f);
            yield return Seg(c, c.Slot.anchoredPosition, c.rot, low, -8f, 0.36f, EaseInOut);
            yield return Seg(c, low, -8f, Vector2.zero, 0f, 0.44f, EaseOut);
            ApplyDepth(c, true);
            if (shakeAfter) StartShake(c);
        }

        void StartShake(FeedbackCardView c)
        {
            if (c.shake != null) StopCoroutine(c.shake);
            c.shake = StartCoroutine(Shake(c));
        }

        static IEnumerator Shake(FeedbackCardView c)
        {
            float[] xs = { 0, -10, 9, -7, 5, -2, 0 };
            float[] rs = { 0, 1.6f, -1.4f, 1f, -0.8f, 0.3f, 0 };
            const float dur = 0.55f;
            for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
            {
                float f = t / dur * (xs.Length - 1);
                int k = Mathf.Min((int)f, xs.Length - 2);
                float u = f - k;
                c.body.anchoredPosition = new Vector2(Mathf.Lerp(xs[k], xs[k + 1], u), 0);
                c.body.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(rs[k], rs[k + 1], u));
                yield return null;
            }
            c.body.anchoredPosition = Vector2.zero;
            c.body.localRotation = Quaternion.identity;
        }

        void RefreshAll(bool snap)
        {
            var rem = Remaining();
            foreach (var c in cards) RefreshCard(c, rem);
            if (snap)
            {
                foreach (var c in cards)
                {
                    ApplyDepth(c, true);
                    GetRest(c, out var p, out var r);
                    SetPose(c, p, r);
                }
                ApplyOrder(-1, -1);
            }
            UpdatePill(rem.Count);
        }

        void ApplyDepth(FeedbackCardView c, bool setAlpha)
        {
            int d = Depth(c.questionIndex);
            if (setAlpha) c.slotGroup.alpha = d <= 4 ? 1f : 0f;
            c.slotGroup.blocksRaycasts = d == 0;
            c.slotGroup.interactable = d == 0;
            c.contentGroup.alpha = d == 0 ? 1f : (d == 1 ? 0.5f : 0.4f);
            c.frost.color = WithAlpha(c.frost.color, d == 0 ? 0f : (d == 1 ? 0.45f : 0.6f));
            c.shadow.color = WithAlpha(c.shadow.color, d == 0 ? 0.18f : 0f);
        }

        void RefreshCard(FeedbackCardView c, List<int> rem)
        {
            var th = theme;
            var q = questions[c.questionIndex];
            var a = A(q);
            bool done = Done(c.questionIndex);
            var empty = WithAlpha(th.line, 0.18f);

            for (int k = 0; k < c.ticks.Count; k++)
            {
                int ii = c.tickQuestion[k];
                c.ticks[k].color = ii == c.questionIndex ? th.current : Answered(ii) ? th.line : fix && !Done(ii) ? th.warningTick : empty;
            }

            switch (q.type)
            {
                case QuestionType.Scale:
                    c.counter.text = "";
                    foreach (var pv in c.points)
                    {
                        bool on = a.scale == pv.value;
                        pv.fill.color = on ? th.accent : Color.white;
                        pv.ring.color = on ? th.ink : th.pointRing;
                        pv.shadow.enabled = on;
                        pv.fill.rectTransform.localScale = Vector3.one * (on ? 1.1f : 1f);
                    }
                    break;

                case QuestionType.Multi:
                case QuestionType.Single:
                    bool single = q.type == QuestionType.Single;
                    c.counter.text = single ? "" : FeedbackTexts.Fill(texts.selectedCount, N, a.sel.Count, q.min, q.max);
                    bool atMax = !single && q.max > 0 && a.sel.Count >= q.max;
                    foreach (var ov in c.options)
                    {
                        var opt = q.options.Find(x => x.id == ov.optionId);
                        bool on = a.sel.Contains(ov.optionId);
                        bool dis = !on && atMax && (opt == null || !opt.exclusive);
                        ov.background.color = on ? WithAlpha(th.accent, 0.3f) : new Color(1, 1, 1, 0);
                        ov.box.color = on ? th.line : Color.white;
                        ov.check.enabled = on;
                        ov.group.alpha = dis ? 0.45f : 1f;
                        ov.button.interactable = !dis;
                        if (ov.otherHolder != null && ov.otherHolder.activeSelf != on) ov.otherHolder.SetActive(on);
                    }
                    break;

                default:
                    c.counter.text = $"{a.text.Length}/{q.maxChars}";
                    break;
            }

            string label = texts.next, icon = "next";
            if (!fix)
            {
                if (c.questionIndex == N - 1) { label = texts.send; icon = "send"; }
            }
            else if (rem.Count == 0) { label = texts.send; icon = "send"; }
            else if (!done) { label = texts.answerFirst; icon = null; }
            else { label = texts.goToRemaining; }
            c.primaryLabel.text = FeedbackTexts.Fill(label, N);
            c.primaryIcon.gameObject.SetActive(icon != null);
            if (icon != null)
            {
                c.primaryIcon.sprite = icon == "send" ? iconSend : iconNext;
                var custom = icon == "send" ? customIcons.send : customIcons.next;
                c.primaryIcon.color = custom != null && customIcons.keepOriginalColors ? Color.white : th.ink;
            }
            c.primaryFill.color = icon == "send" ? th.accent : th.button;

            bool canBack = c.questionIndex > 0;
            c.back.interactable = canBack;
            c.backGroup.alpha = canBack ? 1f : 0.35f;

            bool red = fix && !done;
            c.border.color = red ? th.warning : th.line;
            c.glow.enabled = red;
        }

        void UpdatePill(int remaining)
        {
            if (pill == null) return;
            if (!fix)
            {
                pill.gameObject.SetActive(false);
                return;
            }
            pillText.text = FeedbackTexts.Fill(remaining == 0 ? texts.allAnswered : texts.remaining, N, remaining);
            bool appearing = !pill.gameObject.activeSelf;
            pill.gameObject.SetActive(true);
            if ((appearing || remaining != lastRemaining) && Application.isPlaying && isActiveAndEnabled)
                StartCoroutine(Pop(pill));
            lastRemaining = remaining;
        }

        static IEnumerator Pop(RectTransform rt)
        {
            const float dur = 0.45f;
            for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
            {
                rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, EaseOutBack(t / dur));
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        void ShowModal(FeedbackModalView m)
        {
            HideModals();
            if (m == null || m.root == null) return;
            m.root.SetActive(true);
            m.root.transform.SetAsLastSibling();
            currentModal = m;
            StartCoroutine(PopIn(m));
            if (m.drainFill != null)
            {
                m.drainFill.anchorMax = Vector2.one;
                if (m == thanksModal) StartCoroutine(Drain(m.drainFill, 2.8f));
            }
        }

        void HideModals()
        {
            foreach (var m in new[] { confirmModal, thanksModal, leaveModal })
                if (m != null && m.root != null) m.root.SetActive(false);
            currentModal = null;
        }

        static IEnumerator PopIn(FeedbackModalView m)
        {
            const float dur = 0.45f;
            var box = m.box;
            for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
            {
                float u = t / dur;
                float e = EaseOutBack(u);
                if (m.group != null) m.group.alpha = Mathf.Clamp01(u * 2.2f);
                box.localScale = Vector3.one * Mathf.LerpUnclamped(0.85f, 1f, e);
                box.localRotation = Quaternion.Euler(0, 0, Mathf.LerpUnclamped(3f, 0f, e));
                box.anchoredPosition = new Vector2(0, Mathf.LerpUnclamped(-34f, 0f, e));
                yield return null;
            }
            if (m.group != null) m.group.alpha = 1f;
            box.localScale = Vector3.one;
            box.localRotation = Quaternion.identity;
            box.anchoredPosition = Vector2.zero;
        }

        static IEnumerator Drain(RectTransform fill, float dur)
        {
            for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
            {
                fill.anchorMax = new Vector2(1f - t / dur, 1f);
                yield return null;
            }
            fill.anchorMax = new Vector2(0f, 1f);
        }

        void Submit()
        {
            string json = BuildJson();
            onSubmit.Invoke(json);
            if (!string.IsNullOrEmpty(submitUrl)) Post(json);
            ShowModal(thanksModal);
            StartCoroutine(CloseAfter(2.8f));
        }

        IEnumerator CloseAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            Close();
        }

        void Post(string json)
        {
            var req = new UnityWebRequest(submitUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SendWebRequest().completed += _ =>
            {
                if (req.result != UnityWebRequest.Result.Success)
                    Debug.LogWarning("[FeedbackSurvey] Failed to send answers: " + req.error);
                req.Dispose();
            };
        }

        string BuildJson()
        {
            var sb = new StringBuilder();
            sb.Append("{\"submittedAt\":").Append(Str(DateTime.UtcNow.ToString("o"))).Append(",\"answers\":[");
            for (int i = 0; i < N; i++)
            {
                var q = questions[i];
                var a = A(q);
                if (i > 0) sb.Append(',');
                string sec = q.section >= 0 && q.section < sections.Count ? sections[q.section].name : "";
                sb.Append("{\"id\":").Append(Str(q.id))
                  .Append(",\"section\":").Append(Str(Plain(sec)))
                  .Append(",\"type\":").Append(Str(q.type.ToString().ToLowerInvariant()))
                  .Append(",\"value\":");
                switch (q.type)
                {
                    case QuestionType.Scale:
                        sb.Append(a.scale > 0 ? a.scale.ToString() : "null");
                        break;
                    case QuestionType.Single:
                        sb.Append(a.sel.Count > 0 ? Str(a.sel[0]) : "null");
                        var otherOne = q.options.Find(o => o.isOther);
                        if (otherOne != null && a.sel.Contains(otherOne.id)) sb.Append(",\"other\":").Append(Str(a.other));
                        break;
                    case QuestionType.Multi:
                        sb.Append('[').Append(string.Join(",", a.sel.Select(Str))).Append(']');
                        var other = q.options.Find(o => o.isOther);
                        if (other != null && a.sel.Contains(other.id)) sb.Append(",\"other\":").Append(Str(a.other));
                        break;
                    default:
                        sb.Append(Str(a.text));
                        break;
                }
                sb.Append('}');
            }
            sb.Append("]}");
            return sb.ToString();
        }

        static string Plain(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("|", "");

        static string Str(string s)
        {
            if (s == null) return "null";
            var sb = new StringBuilder("\"");
            foreach (char ch in s)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (ch < 0x20) sb.Append("\\u").Append(((int)ch).ToString("x4"));
                        else sb.Append(ch);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }
    }
}