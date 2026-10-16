using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlatuSalmon.Feedback
{
    [Serializable]
    public class FeedbackTheme
    {
        [Tooltip("Screen background")] public Color background = new Color32(0xDC, 0xEB, 0xF7, 0xFF);
        [Tooltip("Main text")] public Color ink = new Color32(0x5A, 0x3B, 0x22, 0xFF);
        [Tooltip("Borders")] public Color line = new Color32(0x7B, 0x56, 0x34, 0xFF);
        [Tooltip("Secondary text")] public Color muted = new Color32(0x7A, 0x5A, 0x3E, 0xFF);
        [Tooltip("Selected choice / Send button")] public Color accent = new Color32(0xF7, 0xB9, 0x55, 0xFF);
        [Tooltip("Next button")] public Color button = new Color32(0xFF, 0xD5, 0x8A, 0xFF);
        [Tooltip("Progress tick of the current question")] public Color current = new Color32(0xF2, 0xA3, 0x3A, 0xFF);
        [Tooltip("Border of unanswered cards")] public Color warning = new Color32(0xD9, 0x48, 0x3B, 0xFF);
        [Tooltip("Progress tick of unanswered questions")] public Color warningTick = new Color32(0xE5, 0x8B, 0x80, 0xFF);
        [Tooltip("Close button / label / popup background")] public Color cream = new Color32(0xFF, 0xF8, 0xE8, 0xFF);
        [Tooltip("Scrollbar")] public Color scrollbar = new Color32(0xC9, 0xA2, 0x7A, 0xFF);
        [Tooltip("Ring of the 1-5 circles")] public Color pointRing = new Color32(0xB0, 0x8C, 0x6A, 0xFF);
        [Tooltip("Placeholder text in input fields")] public Color placeholder = new Color32(0xA4, 0x87, 0x6B, 0xFF);
    }

    [Serializable]
    public class FeedbackTexts
    {
        [Header("Card buttons (change while playing)")]
        public string back = "ย้อนกลับ";
        public string next = "ถัดไป";
        [Tooltip("Main button on the last card, or once everything is answered")] public string send = "ส่งคำตอบ";
        [Tooltip("Main button on a card that bounced back unanswered")] public string answerFirst = "ตอบข้อนี้ก่อน";
        [Tooltip("Main button when this card is done but others are not")] public string goToRemaining = "ข้อที่ยังเหลือ";

        [Header("Counters and labels (change while playing)")]
        [Tooltip("Top-right of choice cards. {count} = how many are ticked")] public string selectedCount = "เลือกแล้ว {count} ข้อ";
        [Tooltip("Label at the top-left after pressing send too early. {count} = questions left")] public string remaining = "ยังเหลืออีก {count} คำถาม";
        [Tooltip("Same label once everything is answered")] public string allAnswered = "ครบแล้ว";

        [Header("Hints under each question")]
        public string scaleHint = "เลือก 1 ระดับ";
        [Tooltip("Pick-one question")] public string singleHint = "เลือก 1 ข้อ";
        [Tooltip("Added after the hint of questions marked Optional")] public string optionalTag = " (ไม่บังคับ)";
        [Tooltip("Choice question without a maximum. {min} = minimum")] public string multiHintMin = "เลือกอย่างน้อย {min} ข้อ";
        [Tooltip("Choice question with a maximum. {min} / {max}")] public string multiHintRange = "เลือก {min} - {max} ข้อ";
        public string textHint = "พิมพ์คำตอบได้อิสระ";
        public string textPlaceholder = "เช่น อยากให้มีเมืองใหม่ เมนูใหม่ หรือแต่งร้านได้...";
        [Tooltip("Placeholder of the extra field under an \"Other\" choice")] public string otherPlaceholder = "ระบุสั้นๆ...";

        [Header("Confirm-send popup")]
        public string confirmTitle = "ต้องการส่งคำตอบเลยไหม?";
        [TextArea(2, 4)] public string confirmMessage = "ถ้าส่งแล้วจะกลับมาแก้คำตอบไม่ได้นะ";
        public string confirmCancel = "ยกเลิก";
        public string confirmOk = "ยืนยันส่ง";

        [Header("Thank-you popup")]
        public string thanksTitle = "ขอบคุณสำหรับFeedback!";
        [TextArea(2, 4)] public string thanksMessage = "ส่งคำตอบเรียบร้อยแล้ว\nทุก Feedback ของคุณช่วยให้เราพัฒนา Platu Salmon ให้ดียิ่งขึ้น";
        public string thanksClosing = "กำลังพากลับเข้าเกม...";

        [Header("Leave popup (X button)")]
        public string leaveTitle = "ออกจากแบบสอบถาม?";
        [TextArea(2, 4)] public string leaveMessage = "คำตอบที่ทำไว้จะไม่ถูกบันทึก";
        public string leaveCancel = "ทำต่อ";
        public string leaveOk = "ออก";

        public static string Fill(string s, int n = 0, int count = 0, int min = 0, int max = 0)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return FeedbackUI.T(s.Replace("{n}", n.ToString()).Replace("{count}", count.ToString())
                .Replace("{min}", min.ToString()).Replace("{max}", max.ToString()));
        }
    }

    [Serializable]
    public class FeedbackIcons
    {
        [Tooltip("Back button arrow")] public Sprite back;
        [Tooltip("Next button arrow")] public Sprite next;
        [Tooltip("Send button icon (last card)")] public Sprite send;
        [Tooltip("Close (X) button")] public Sprite close;
        [Tooltip("Tick inside a selected checkbox")] public Sprite check;
        [Tooltip("Picture on the confirm-send popup")] public Sprite confirmPopup;
        [Tooltip("Picture on the thank-you popup")] public Sprite thanksPopup;
        [Tooltip("Keep each custom icon's own colors instead of tinting it brown")] public bool keepOriginalColors = true;
    }

    [Serializable]
    public class FeedbackPointView
    {
        public int value;
        public Button button;
        public Image fill;
        public Image ring;
        public Shadow shadow;
    }

    [Serializable]
    public class FeedbackOptionView
    {
        public string optionId;
        public Button button;
        public Image background;
        public Image box;
        public Image check;
        public CanvasGroup group;
        public GameObject otherHolder;
        public TMP_InputField other;
    }

    [Serializable]
    public class FeedbackModalView
    {
        public GameObject root;
        public CanvasGroup group;
        public RectTransform box;
        public Button cancel;
        public Button ok;
        [Tooltip("Countdown bar (thanks popup only)")] public RectTransform drainFill;
    }

    [DisallowMultipleComponent]
    public class FeedbackCardView : MonoBehaviour
    {
        [Tooltip("Which question this card shows (starts at 0)")] public int questionIndex;

        [Header("Position when behind the stack")]
        public Vector2 restOffset;
        public float restRotation;

        [Header("Card parts")]
        public RectTransform body;
        public CanvasGroup slotGroup;
        public CanvasGroup contentGroup;
        public Image shadow;
        public Image glow;
        public Image paper;
        public Image frost;
        public Image border;
        public List<Image> ticks = new List<Image>();
        public List<int> tickQuestion = new List<int>();
        public TextMeshProUGUI counter;
        public List<FeedbackPointView> points = new List<FeedbackPointView>();
        public List<FeedbackOptionView> options = new List<FeedbackOptionView>();
        public TMP_InputField input;

        [Header("Footer buttons")]
        public Button back;
        public CanvasGroup backGroup;
        public Button primary;
        public Image primaryFill;
        public TextMeshProUGUI primaryLabel;
        public Image primaryIcon;

        [NonSerialized] public float rot;
        [NonSerialized] public Coroutine move;
        [NonSerialized] public Coroutine shake;

        public RectTransform Slot => (RectTransform)transform;
    }
}
