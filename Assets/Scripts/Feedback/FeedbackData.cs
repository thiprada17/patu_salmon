using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlatuSalmon.Feedback
{
    public enum QuestionType { Scale, Multi, Text, Single }

    [Serializable]
    public class FeedbackSection
    {
        public string name;
        public Color paper = Color.white;
    }

    [Serializable]
    public class FeedbackOption
    {
        public string id;
        public string label;
        public string sub;
        [Tooltip("Selecting this clears all other choices (e.g. \"not bored at all\")")]
        public bool exclusive;
        [Tooltip("An \"Other\" choice with an extra text field")]
        public bool isOther;
    }

    [Serializable]
    public class FeedbackQuestion
    {
        public string id;
        [Tooltip("Section index this question belongs to (starts at 0)")]
        public int section;
        public QuestionType type;
        [Tooltip("Tick if the player may skip this question")] public bool optional;
        [TextArea(2, 4)] public string text;
        [Tooltip("Scale: label for level 1")] public string low;
        [Tooltip("Scale: label for level 5")] public string high;
        [Tooltip("Multi: minimum number of choices")] public int min = 1;
        [Tooltip("Multi: maximum number of choices (0 = no limit)")] public int max;
        [Tooltip("Text: maximum characters")] public int maxChars = 300;
        public List<FeedbackOption> options = new List<FeedbackOption>();
    }

    public static class FeedbackDefaults
    {
        public static void Fill(List<FeedbackSection> sections, List<FeedbackQuestion> questions)
        {
            sections.Clear();
            questions.Clear();

            sections.Add(Sec("ข้อมูลทั่วไป", "#FFF9EE"));
            sections.Add(Sec("ประสบการณ์การเล่นเกม", "#FFF3D6"));
            sections.Add(Sec("การออกแบบและบรรยากาศของเกม", "#E6F4EE"));
            sections.Add(Sec("การเรียนรู้ภาษาอังกฤษผ่านเกม", "#FDEADB"));
            sections.Add(Sec("ความคิดเห็นและความพึงพอใจโดยรวม", "#FBEAE7"));

            questions.Add(Single("age", 0, "อายุ",
                Opt("u15", "ต่ำกว่า 15 ปี"),
                Opt("15-18", "15 - 18 ปี"),
                Opt("19-22", "19 - 22 ปี"),
                Opt("23-30", "23 - 30 ปี"),
                Opt("31-45", "31 - 45 ปี"),
                Opt("46-60", "46 - 60 ปี"),
                Opt("61+", "61 ปีขึ้นไป")));

            questions.Add(Scale("tutorial", 1, "ช่วงสอนเล่น (tutorial) ช่วยให้ท่านเข้าใจวิธีเล่นเกม?",
                "ไม่เห็นด้วยอย่างยิ่ง", "เห็นด้วยอย่างยิ่ง"));
            questions.Add(Scale("order_time", 1, "ท่านคิดว่าระยะเวลาที่กำหนดให้ในการทำออร์เดอร์แต่ละรายการมีความเหมาะสมเพียงใด?",
                "ระยะเวลาสั้นเกินไป", "ระยะเวลานานเกินไป"));
            questions.Add(Scale("difficulty", 1, "ท่านคิดว่าระดับความยากของเกมมีความเหมาะสมเพียงใด?",
                "ง่ายเกินไป", "ยากเกินไป"));
            questions.Add(Scale("reward", 1, "เงินหรือรางวัลที่ได้คุ้มกับความพยายาม",
                "น้อยเกินไป เล่นแล้วไม่คุ้ม", "เยอะเกินไป ได้เงินรางวัลง่ายเกิน"));
            questions.Add(Multi("boring", 1, "ระหว่างเล่นเกม Platu Salmon มีช่วงใดบ้างที่ทำให้คุณรู้สึกเบื่อหรือรู้สึกว่าเกมดำเนินไปช้าเกินไป?", 1, 0,
                Opt("none", "ไม่รู้สึกเบื่อในระหว่างเล่นเกม", exclusive: true),
                Opt("tutorial", "ช่วงแนะนำวิธีเล่น", "เนื่องจากใช้เวลานานหรือมีคำอธิบายมากเกินไป"),
                Opt("waiting", "ช่วงรอลูกค้าเข้าร้าน", "เนื่องจากไม่มีอะไรให้ทำระหว่างรอ"),
                Opt("ordering", "ช่วงทำออร์เดอร์", "เนื่องจากรูปแบบการเล่นซ้ำเดิมมากเกินไป"),
                Opt("summary", "ช่วงแสดงผลสรุปหลังจบด่าน", "เนื่องจากใช้เวลานานหรือมีข้อมูลไม่น่าสนใจ"),
                Opt("vocab_review", "ช่วงทบทวนคำศัพท์ภาษาอังกฤษ", "เนื่องจากรูปแบบการนำเสนอไม่น่าสนใจ"),
                Opt("grinding", "ช่วงสะสมเงินเพื่อปลดล็อกเมืองใหม่", "เนื่องจากต้องเล่นซ้ำหลายครั้งหรือใช้เวลานานเกินไป"),
                Opt("other", "อื่นๆ", isOther: true)));

            questions.Add(Scale("art", 2, "ท่านชื่นชอบต่อการออกแบบตัวละคร ฉากภายในเกม และสิ่งของต่าง ๆ ในเกมมากน้อยเพียงใด?",
                "ไม่ชื่นชอบเลย", "ชื่นชอบมาก"));
            questions.Add(Scale("ui_clarity", 2, "ท่านคิดว่าปุ่มควบคุม สัญลักษณ์ และข้อมูลต่าง ๆ บนหน้าจอเกมมีความชัดเจนและเข้าใจง่ายเพียงใด?",
                "เข้าใจยากมาก", "เข้าใจง่ายมาก"));
            questions.Add(Scale("music", 2, "ท่านคิดว่าเพลงและเสียงประกอบมีความเหมาะสมกับบรรยากาศและรูปแบบการเล่นของเกมมากน้อยเพียงใด?",
                "น้อยที่สุด", "มากที่สุด"));
            questions.Add(Scale("atmosphere", 2, "บรรยากาศและการตกแต่งร้านอาหารภายในเกมช่วยให้คุณรู้สึกเหมือนได้เดินทางไปยังเมืองนั้นจริง ๆ มากน้อยเพียงใด?",
                "น้อยที่สุด", "มากที่สุด"));

            questions.Add(Scale("vocab_recall", 3, "หลังจากเล่นเกม ท่านสามารถจดจำคำศัพท์ภาษาอังกฤษที่พบในรายการออร์เดอร์ได้มากน้อยเพียงใด?",
                "แทบจดจำไม่ได้เลย", "จดจำได้เกือบทั้งหมด"));
            questions.Add(Scale("vocab_level", 3, "ท่านคิดว่าระดับความยากของคำศัพท์ภาษาอังกฤษในเกม Platu Salmon มีความเหมาะสมเพียงใด?",
                "ง่ายเกินไป", "ยากเกินไป"));
            questions.Add(Scale("vs_other_methods", 3, "เมื่อเปรียบเทียบกับวิธีฝึกภาษาอังกฤษที่ท่านเคยใช้ ท่านรู้สึกว่าการเรียนรู้คำศัพท์ภาษาอังกฤษผ่านเกม Platu Salmon เป็นอย่างไร?",
                "น่าเบื่อกว่า", "สนุกกว่า"));

            questions.Add(Single("favorite", 4, "ท่านชื่นชอบส่วนใดของเกม Platu Salmon มากที่สุด?",
                Opt("customers", "ตัวละครลูกค้าที่น่ารัก"),
                Opt("cooking", "การรับออร์เดอร์และการทำอาหาร"),
                Opt("unlock", "การปลดล็อกเมืองหรือสถานที่ใหม่"),
                Opt("learning", "การเรียนรู้คำศัพท์ภาษาอังกฤษระหว่างเล่น"),
                Opt("vocab_summary", "หน้าสรุปคำศัพท์ภาษาอังกฤษ"),
                Opt("graphics", "กราฟิกและบรรยากาศภายในร้านอาหาร"),
                Opt("music", "เพลงและเสียงประกอบ"),
                Opt("other", "อื่นๆ", isOther: true)));
            questions.Add(Multi("problems", 4, "ระหว่างเล่นเกม Platu Salmon ท่านพบปัญหาหรืออุปสรรคใดบ้าง?", 1, 0,
                Opt("crash", "เกมค้างหรือปิดตัวเองระหว่างเล่น"),
                Opt("loading", "เกมใช้เวลาโหลดนานเกินไป"),
                Opt("unresponsive", "กดปุ่มแล้วเกมไม่ตอบสนอง"),
                Opt("small_buttons", "ปุ่มมีขนาดเล็กหรือกดยาก"),
                Opt("small_text", "ตัวอักษรมีขนาดเล็กหรืออ่านยาก"),
                Opt("cant_find", "หาปุ่มหรือเมนูที่ต้องการไม่พบ"),
                Opt("audio", "เพลงและเสียงประกอบ"),
                Opt("what_next", "ไม่เข้าใจว่าต้องทำอะไรต่อ"),
                Opt("order_unclear", "ไม่เข้าใจรายการอาหารที่ลูกค้าสั่ง"),
                Opt("not_enough_time", "มีเวลาไม่เพียงพอในการทำออร์เดอร์"),
                Opt("vocab_hard", "คำศัพท์ภาษาอังกฤษเข้าใจยากเกินไป"),
                Opt("want_translation", "ต้องการคำแปลภาษาไทยเพิ่มเติม"),
                Opt("want_audio", "ต้องการเสียงอ่านคำศัพท์ภาษาอังกฤษ"),
                Opt("none", "ไม่พบปัญหาระหว่างเล่นเกม", exclusive: true),
                Opt("other", "อื่นๆ", isOther: true)));
            questions.Add(Scale("satisfaction", 4, "โดยภาพรวมแล้ว ท่านพึงพอใจกับการเล่นเกม Platu Salmon มากน้อยเพียงใด?",
                "ไม่พึงพอใจอย่างยิ่ง", "พึงพอใจมากที่สุด"));
            var suggestions = Text("suggestions", 4, "ท่านต้องการให้เกม Platu Salmon เพิ่มฟีเจอร์หรือปรับปรุงส่วนใดเพิ่มเติม?", 300);
            suggestions.optional = true;
            questions.Add(suggestions);
        }

        static FeedbackSection Sec(string name, string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return new FeedbackSection { name = name, paper = c };
        }

        static FeedbackQuestion Scale(string id, int s, string text, string low, string high) =>
            new FeedbackQuestion { id = id, section = s, type = QuestionType.Scale, text = text, low = low, high = high };

        static FeedbackQuestion Multi(string id, int s, string text, int min, int max, params FeedbackOption[] opts)
        {
            var q = new FeedbackQuestion { id = id, section = s, type = QuestionType.Multi, text = text, min = min, max = max };
            q.options.AddRange(opts);
            return q;
        }

        static FeedbackQuestion Single(string id, int s, string text, params FeedbackOption[] opts)
        {
            var q = new FeedbackQuestion { id = id, section = s, type = QuestionType.Single, text = text, min = 1, max = 1 };
            q.options.AddRange(opts);
            return q;
        }

        static FeedbackQuestion Text(string id, int s, string text, int maxChars) =>
            new FeedbackQuestion { id = id, section = s, type = QuestionType.Text, text = text, maxChars = maxChars };

        static FeedbackOption Opt(string id, string label, string sub = null, bool exclusive = false, bool isOther = false) =>
            new FeedbackOption { id = id, label = label, sub = sub, exclusive = exclusive, isOther = isOther };
    }
}