
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("Backend")]
    [SerializeField] private string backendUrl = "http://127.0.0.1:8000";

    [Header("Recipes")]
    [SerializeField] private List<RecipeData> recipes;

    [Header("ตั้งค่าทิป")]
    [SerializeField, Min(0)] private int maxTip = 20;

    [Header("ค่าปรับ")]
    [SerializeField, Min(0)] private int angryPenalty = 5;

    [Header("Navbar")]
    [SerializeField] private TMP_Text totalCoinsText;
    [SerializeField] private TMP_Text daysText;
    [SerializeField] private TMP_Text orderCoinsText;
    [SerializeField] private TMP_Text tipCoinsText;
    [SerializeField] private TMP_Text totalcustomerservedText;

    [Header("หน้าต่างสรุปรายได้")]
    [SerializeField] private OrderEarningPopup earningPopup;

    [Header("หน้าต่างสรุปกะ")]
    [SerializeField] private GameObject shiftSummaryPopup;
    [SerializeField] private TMP_Text shiftSummaryText;
    [SerializeField, Min(1)] private int daysPerShift = 10;

    // จำนวนเหรียญสะสมที่ได้รับจาก Backend
    public int SavedCoins { get; private set; }

    // รายได้และค่าปรับของวันปัจจุบัน
    public int OrderCoins { get; private set; }
    public int TipCoins { get; private set; }
    public int PenaltyCoins { get; private set; }

    // จำนวนลูกค้าสะสมที่ได้รับจาก Backend
    public int CustomersServed { get; private set; }

    // จำนวนลูกค้าที่เสิร์ฟในวันปัจจุบัน
    public int TodayCustomersServed { get; private set; }

    // จำนวนวันที่ทำงานสะสม
    public int WorkDays { get; private set; }

    // รายได้สุทธิของวันปัจจุบัน
    public int ShiftCoins => OrderCoins + TipCoins - PenaltyCoins;

    // จำนวนเหรียญทั้งหมด รวมรายได้สุทธิของวันปัจจุบัน
    public int TotalCoins => Mathf.Max(0, SavedCoins + ShiftCoins);

    private bool isLoading;
    private bool isSaving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            GameLogger.Warn("CoinManager", "พบ CoinManager ซ้ำ กำลังลบตัวที่ซ้ำ");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        GameLogger.Info("CoinManager", "Awake ทำงานแล้ว ตั้งค่า Instance สำเร็จ");
    }

    private void Start()
    {
        GameLogger.Info("Economy", "CoinManager Start เริ่มทำงาน");
        // แสดงค่าเริ่มต้นก่อนโหลดข้อมูลจาก Backend
        UpdateNavbar();

        // โหลดข้อมูลผู้เล่น
        StartCoroutine(LoadStats());
    }

    // โหลดข้อมูลเหรียญ จำนวนวันที่ทำงาน และจำนวนลูกค้าสะสม
    private IEnumerator LoadStats()
    {
        string userId = LoginManager.UserId;

        GameLogger.Info(
            "Economy",
            $"เริ่ม LoadStats | UserID={userId} | URL={backendUrl}/player/stats/{userId}"
        );

        if (string.IsNullOrEmpty(userId))
        {
            GameLogger.Error("Economy", "ไม่พบ UserID");
            yield break;
        }

        isLoading = true;

        using (UnityWebRequest request = UnityWebRequest.Get(
            $"{backendUrl}/player/stats/{userId}"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                GameLogger.Error(
                    "Economy",
                    $"โหลดข้อมูลไม่สำเร็จ: {request.responseCode} {request.downloadHandler.text}"
                );

                isLoading = false;
                yield break;
            }

            PlayerStatsResponse data =
                JsonUtility.FromJson<PlayerStatsResponse>(
                    request.downloadHandler.text
                );

            if (data == null)
            {
                GameLogger.Error("Economy", "ไม่สามารถอ่านข้อมูล Stats ได้");
                isLoading = false;
                yield break;
            }

            // เก็บยอดสะสมจาก Backend
            SavedCoins = data.coin;
            WorkDays = data.work_days;
            CustomersServed = data.total_customers_served;

            TodayCustomersServed = 0;

            UpdateNavbar();

            GameLogger.Info(
                "Economy",
                $"โหลดข้อมูลเข้าแล้ว | Coins={SavedCoins}, Days={WorkDays}"
            );

            GameLogger.Info(
                "Economy",
                $"GET Response: {request.downloadHandler.text}"
            );

            if (request.result != UnityWebRequest.Result.Success)
            {
                GameLogger.Error(
                    "Economy",
                    $"Load stats failed | " +
                    $"Result={request.result} | " +
                    $"HTTP={request.responseCode} | " +
                    $"Error={request.error} | " +
                    $"URL={request.url} | " +
                    $"Response={request.downloadHandler.text}"
                );

                // อัปเดตหน้าจอ แม้โหลดข้อมูลไม่สำเร็จ
                UpdateNavbar();

                isLoading = false;
                yield break;
            }
        }

        isLoading = false;
    }

    // คำนวณรายได้เมื่อเสิร์ฟลูกค้าสำเร็จ
    public void EarnCoins(
        RecipeType recipeType,
        float waitTime,
        float maxWaitTime,
        bool isAngry)
    {
        RecipeData recipe = recipes.Find(
            r => r != null && r.recipeType == recipeType
        );

        if (recipe == null)
        {
            GameLogger.Warn("Order", $"ไม่พบ RecipeData: {recipeType}");
            return;
        }

        int price = recipe.price;

        // ยิ่งใช้เวลาน้อยเมื่อเทียบกับเวลาสูงสุด ยิ่งมีโอกาสได้ทิปสูง
        float speedRatio = maxWaitTime > 0f
            ? Mathf.Clamp01(1f - waitTime / maxWaitTime)
            : 0f;

        int tipLimit = Mathf.RoundToInt(maxTip * speedRatio);

        int tip = isAngry
            ? 0
            : UnityEngine.Random.Range(0, tipLimit + 1);

        // แสดงค่าปรับในหน้าต่างสรุป แต่ยังไม่บันทึกค่าปรับตรงนี้
        int penaltyForPopup = isAngry ? angryPenalty : 0;

        // เพิ่มรายได้และนับจำนวนลูกค้าของวันปัจจุบัน
        OrderCoins += price;
        TipCoins += tip;
        TodayCustomersServed++;

        UpdateNavbar();

        // แสดงหน้าต่างสรุปรายได้ของออร์เดอร์
        if (earningPopup != null)
        {
            earningPopup.Show(price, tip, penaltyForPopup);
        }

        GameLogger.DebugLog(
            "Economy",
            $"Order={price}, Tip={tip}, " +
            $"TodayCustomers={TodayCustomersServed}, TotalCoins={TotalCoins}"
        );
    }

    // เพิ่มค่าปรับเมื่อลูกค้าไม่พอใจ
    public void ApplyAngryPenalty()
    {
        PenaltyCoins += angryPenalty;
        UpdateNavbar();
    }

    // บันทึกผลการทำงานของวันปัจจุบัน
    public void FinishWorkDay(Action<bool> onComplete)
    {
        if (isLoading || isSaving)
        {
            GameLogger.Warn("Economy", "กำลังโหลดหรือบันทึกข้อมูลอยู่");
            onComplete?.Invoke(false);
            return;
        }

        if (string.IsNullOrEmpty(LoginManager.UserId))
        {
            GameLogger.Error("Economy", "ไม่พบ UserID ไม่สามารถบันทึกข้อมูลได้");
            onComplete?.Invoke(false);
            return;
        }

        StartCoroutine(SaveWorkDay(onComplete));
    }


    private IEnumerator SaveWorkDay(Action<bool> onComplete)
    {
        isSaving = true;

        // ส่งยอดสะสมใหม่ให้ตรงกับ Schema ของ Backend
        FinishWorkDayRequest data = new FinishWorkDayRequest
        {
            user_id = LoginManager.UserId,
            coin = TotalCoins,
            work_days = WorkDays + 1,
            total_customers_served = CustomersServed + TodayCustomersServed
        };

        string json = JsonUtility.ToJson(data);
        byte[] body = Encoding.UTF8.GetBytes(json);

        GameLogger.Info("Economy", $"กำลังบันทึกผลวัน: {json}");

        using (UnityWebRequest request = new UnityWebRequest(
            $"{backendUrl}/player/stats/day", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                GameLogger.Error(
                    "Economy",
                    $"บันทึกข้อมูลไม่สำเร็จ: HTTP {request.responseCode} " +
                    $"{request.downloadHandler.text}"
                );

                isSaving = false;
                onComplete?.Invoke(false);
                yield break;
            }

            FinishWorkDayResponse result =
                JsonUtility.FromJson<FinishWorkDayResponse>(
                    request.downloadHandler.text
                );

            if (result == null)
            {
                GameLogger.Error("Economy", "อ่านผลการบันทึกไม่สำเร็จ");
                isSaving = false;
                onComplete?.Invoke(false);
                yield break;
            }

            // ใช้ยอดที่ Backend ตอบกลับมา
            SavedCoins = result.coin;
            WorkDays = result.work_days;
            CustomersServed = result.total_customers_served;

            // ล้างรายได้วันนี้หลังบันทึกสำเร็จเท่านั้น
            OrderCoins = 0;
            TipCoins = 0;
            PenaltyCoins = 0;
            TodayCustomersServed = 0;

            UpdateNavbar();

            if (daysPerShift > 0 && WorkDays % daysPerShift == 0)
            {
                ShowShiftSummary();
            }

            GameLogger.Info(
                "Economy",
                $"บันทึกสำเร็จ | วัน={WorkDays} | " +
                $"เหรียญ={SavedCoins} | ลูกค้า={CustomersServed}"
            );
        }

        isSaving = false;
        onComplete?.Invoke(true);
    }


    // แสดงสรุปผลการทำงานเมื่อครบจำนวนวันที่กำหนด
    private void ShowShiftSummary()
    {
        if (shiftSummaryText != null)
        {
            shiftSummaryText.text =
                $"วันที่ทำงาน: {WorkDays}\n" +
                $"เหรียญทั้งหมด: {SavedCoins}\n" +
                $"ลูกค้าที่เสิร์ฟสะสม: {CustomersServed}";
        }

        if (shiftSummaryPopup != null)
        {
            shiftSummaryPopup.SetActive(true);
        }
    }

    // ปิดหน้าต่างสรุปกะ
    public void CloseShiftSummary()
    {
        if (shiftSummaryPopup != null)
        {
            shiftSummaryPopup.SetActive(false);
        }
    }
    public void RefreshNavbar()
    {
        UpdateNavbar();
        GameLogger.Info(
    "Economy",
    $"RefreshNavbar | Coins={TotalCoins} | Days={WorkDays} | " +
    $"Customers={CustomersServed + TodayCustomersServed}"
);
    }
    // อัปเดตข้อมูลทั้งหมดบน Navbar
    private void UpdateNavbar()
    {
        // แสดงเหรียญรวม รวมรายได้ของวันปัจจุบัน
        if (totalCoinsText != null)
        {
            totalCoinsText.text = TotalCoins.ToString();
        }

        // แสดงรายได้จากการขายและทิปของวันนี้
        if (orderCoinsText != null)
        {
            orderCoinsText.text = "Order " + OrderCoins;
        }

        if (tipCoinsText != null)
        {
            tipCoinsText.text = "Tips " + TipCoins;
        }

        // แสดงยอดลูกค้าสะสม รวมลูกค้าที่เสิร์ฟวันนี้
        if (totalcustomerservedText != null)
        {
            int totalServed = CustomersServed + TodayCustomersServed;
            totalcustomerservedText.text = "Served " + totalServed;
        }

        // แสดงจำนวนวันที่ทำงานสะสม
        if (daysText != null)
        {
            daysText.text = WorkDays.ToString();
        }
    }

    // รูปแบบข้อมูลที่ได้รับจาก GET /player/stats/{user_id}
    [Serializable]
    private class PlayerStatsResponse
    {
        public string message;
        public string user_id;
        public int coin;
        public int work_days;
        public int work_streak;
        public int total_customers_served;
    }

    // รูปแบบข้อมูลที่ส่งไป POST /player/stats/day

    [Serializable]
    private class FinishWorkDayRequest
    {
        public string user_id;
        public int coin;
        public int work_days;
        public int total_customers_served;
    }


    // รูปแบบข้อมูลที่ได้รับกลับจาก POST /player/stats/day
    [Serializable]
    private class FinishWorkDayResponse
    {
        public string message;
        public string user_id;
        public int coin;
        public int work_days;
        public int total_customers_served;
    }

}