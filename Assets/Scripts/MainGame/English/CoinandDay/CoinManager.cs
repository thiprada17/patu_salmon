
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
    [SerializeField] private string backendUrl = "https://patu-salmon.onrender.com";

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

    //[Header("หน้าต่างสรุปรายได้")]
    //[SerializeField] private OrderEarningPopup earningPopup;

    [Header("หน้าต่างสรุปกะ")]
    [SerializeField] private ShiftEnd shiftEnd;
    [SerializeField, Min(1)] private int daysPerShift = 10;

    [Header("ค่าใช้จ่ายตอนจบกะ")]
    [SerializeField, Min(0)] private int minTicketExpense = 10;
    [SerializeField, Min(0)] private int maxTicketExpense = 50;
    [SerializeField, Min(0)] private int minRentExpense = 20;
    [SerializeField, Min(0)] private int maxRentExpense = 100;

    // ค่าใช้จ่ายของกะล่าสุด
    public int TicketExpense { get; private set; }
    public int RentExpense { get; private set; }

    // ยอดเงินที่บันทึกไว้ใน Backend
    public int SavedCoins { get; private set; }

    // รายได้และค่าปรับของวันปัจจุบัน
    public int OrderCoins { get; private set; }
    public int TipCoins { get; private set; }
    public int PenaltyCoins { get; private set; }

    // จำนวนลูกค้าสะสมที่บันทึกใน Backend
    public int CustomersServed { get; private set; }

    // จำนวนลูกค้าที่เสิร์ฟในวันปัจจุบัน
    public int TodayCustomersServed { get; private set; }

    // รายได้และค่าปรับของวันปัจจุบัน สำหรับแสดงในหน้าสรุปรายวัน
    public int TodayOrderCoins { get; private set; }
    public int TodayTipCoins { get; private set; }
    public int TodayPenaltyCoins { get; private set; }
    public int TodayNetEarning =>
        TodayOrderCoins + TodayTipCoins - TodayPenaltyCoins;

    // จำนวนวันที่ทำงานสะสม
    public int WorkDays { get; private set; }

    // ยอดรายได้สุทธิของวันปัจจุบัน
    public int ShiftCoins => OrderCoins + TipCoins - PenaltyCoins;

    // ยอดรายได้สุทธิของวันนี้หลังหักค่าใช้จ่าย (มีเฉพาะวันครบ 10 วัน)
    public int TotalShiftEarning =>
        OrderCoins + TipCoins - PenaltyCoins
        - TicketExpense - RentExpense;

    private bool isLoading;
    private bool isSaving;
    private int pendingExpenseWorkDay = -1;

    // เงินคงเหลือหลังบันทึกยอดของวันนี้แล้ว
    public int TotalCoins =>
        Mathf.Max(0, SavedCoins + ShiftCoins);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            GameLogger.Warn(
                "CoinManager",
                "พบ CoinManager ซ้ำ กำลังลบตัวที่ซ้ำ"
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        GameLogger.Info(
            "CoinManager",
            "Awake ทำงานแล้ว ตั้งค่า Instance สำเร็จ"
        );
    }

    private void Start()
    {
        GameLogger.Info("Economy", "CoinManager Start เริ่มทำงาน");

        UpdateNavbar();
        StartCoroutine(LoadStats(hasRefreshedSession: false));
    }

    // โหลดข้อมูลผู้เล่นจาก Backend
    private IEnumerator LoadStats(bool hasRefreshedSession)
    {
        string userId = LoginManager.UserId;
        string accessToken = PlayerPrefs.GetString("AccessToken", "");

        GameLogger.Info(
            "Economy",
            $"เริ่ม LoadStats | UserID={userId} | " +
            $"URL={backendUrl}/player/stats/{userId}"
        );

        if (string.IsNullOrEmpty(userId))
        {
            GameLogger.Error("Economy", "ไม่พบ UserID");
            yield break;
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            GameLogger.Error("Economy", "ไม่พบ Access Token");
            yield break;
        }

        isLoading = true;

        using (UnityWebRequest request = UnityWebRequest.Get(
            $"{backendUrl}/player/stats/{userId}"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.responseCode == 401 && !hasRefreshedSession)
                {
                    bool refreshFinished = false;
                    bool refreshSucceeded = false;
                    LoginManager.Instance.RefreshSession(success =>
                    {
                        refreshSucceeded = success;
                        refreshFinished = true;
                    });

                    while (!refreshFinished)
                        yield return null;

                    isLoading = false;
                    if (refreshSucceeded)
                    {
                        yield return LoadStats(hasRefreshedSession: true);
                        yield break;
                    }
                }

                GameLogger.Error(
                    "Economy",
                    $"โหลดข้อมูลไม่สำเร็จ: HTTP {request.responseCode} " +
                    $"{request.downloadHandler.text}"
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
                GameLogger.Error(
                    "Economy",
                    "ไม่สามารถอ่านข้อมูล Stats ได้"
                );

                isLoading = false;
                yield break;
            }

            SavedCoins = data.coin;
            WorkDays = data.work_days;
            CustomersServed = data.total_customers_served;

            // เริ่มนับลูกค้าของวันปัจจุบันใหม่
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
            GameLogger.Warn(
                "Order",
                $"ไม่พบ RecipeData: {recipeType}"
            );

            return;
        }

        int price = recipe.price;

        float speedRatio = maxWaitTime > 0f
            ? Mathf.Clamp01(1f - waitTime / maxWaitTime)
            : 0f;

        int tipLimit = Mathf.RoundToInt(maxTip * speedRatio);

        int tip = isAngry
            ? 0
            : UnityEngine.Random.Range(0, tipLimit + 1);

        int penaltyForPopup = isAngry ? angryPenalty : 0;

        // รายได้สะสมตลอดกะ ไม่รีเซ็ตทุกวัน
        OrderCoins += price;
        TipCoins += tip;
        TodayOrderCoins += price;
        TodayTipCoins += tip;
        TodayCustomersServed++;

        UpdateNavbar();

        //if (earningPopup != null)
        //{
        //    earningPopup.Show(price, tip, penaltyForPopup);
        //}

        GameLogger.DebugLog(
            "Economy",
            $"Order={price}, Tip={tip}, " +
            $"TodayCustomers={TodayCustomersServed}, " +
            $"ShiftOrder={OrderCoins}, ShiftTips={TipCoins}, " +
            $"TotalCoins={TotalCoins}"
        );
    }

    // เพิ่มค่าปรับเมื่อลูกค้าไม่พอใจ
    public void ApplyAngryPenalty()
    {
        PenaltyCoins += angryPenalty;
        TodayPenaltyCoins += angryPenalty;

        UpdateNavbar();

        GameLogger.Info(
            "Economy",
            $"เพิ่มค่าปรับ={angryPenalty} | PenaltyCoins={PenaltyCoins}"
        );
    }

    // เริ่มบันทึกผลของวัน
    public void FinishWorkDay(Action<bool> onComplete)
    {
        if (isLoading || isSaving)
        {
            GameLogger.Warn(
                "Economy",
                "กำลังโหลดหรือบันทึกข้อมูลอยู่"
            );

            onComplete?.Invoke(false);
            return;
        }

        if (string.IsNullOrEmpty(LoginManager.UserId))
        {
            GameLogger.Error(
                "Economy",
                "ไม่พบ UserID ไม่สามารถบันทึกข้อมูลได้"
            );

            onComplete?.Invoke(false);
            return;
        }

        if (string.IsNullOrEmpty(PlayerPrefs.GetString("AccessToken", "")))
        {
            GameLogger.Error("Economy", "ไม่พบ Access Token ไม่สามารถบันทึกข้อมูลได้");
            onComplete?.Invoke(false);
            return;
        }

        StartCoroutine(SaveWorkDay(onComplete, hasRefreshedSession: false));
    }

    private IEnumerator SaveWorkDay(
        Action<bool> onComplete,
        bool hasRefreshedSession)
    {
        isSaving = true;

        int nextWorkDays = WorkDays + 1;
        bool isShiftEnd =
            daysPerShift > 0 && nextWorkDays % daysPerShift == 0;

        if (isShiftEnd)
        {
            // Keep charges fixed if this work day needs to be retried.
            if (pendingExpenseWorkDay != nextWorkDays)
            {
                TicketExpense = UnityEngine.Random.Range(
                    minTicketExpense,
                    maxTicketExpense + 1
                );
                RentExpense = UnityEngine.Random.Range(
                    minRentExpense,
                    maxRentExpense + 1
                );
                pendingExpenseWorkDay = nextWorkDays;
            }
        }
        else
        {
            TicketExpense = 0;
            RentExpense = 0;
            pendingExpenseWorkDay = -1;
        }

        GameLogger.Info(
            "Economy",
            $"ตรวจเงินก่อนบันทึก | " +
            $"SavedCoins={SavedCoins} | " +
            $"OrderCoins={OrderCoins} | " +
            $"TipCoins={TipCoins} | " +
            $"PenaltyCoins={PenaltyCoins} | " +
            $"ShiftCoins={ShiftCoins} | " +
            $"Ticket={TicketExpense} | Rent={RentExpense}"
        );

        int finalCoins = Mathf.Max(
            0,
            SavedCoins + ShiftCoins
            - TicketExpense - RentExpense
        );

        GameLogger.Info(
            "Economy",
            $"ยอดที่จะส่ง Backend = {finalCoins}"
        );

        GameLogger.Info(
            "Economy",
            $"จบกะ={isShiftEnd} | " +
            $"CoinsBefore={TotalCoins} | CoinsAfter={finalCoins}"
        );

        FinishWorkDayRequest data = new FinishWorkDayRequest
        {
            user_id = LoginManager.UserId,
            coin = finalCoins,
            work_days = nextWorkDays,
            total_customers_served =
                CustomersServed + TodayCustomersServed
        };

        string json = JsonUtility.ToJson(data);
        byte[] body = Encoding.UTF8.GetBytes(json);

        GameLogger.Info(
            "Economy",
            $"กำลังบันทึกผลวัน: {json}"
        );

        using (UnityWebRequest request = new UnityWebRequest(
            $"{backendUrl}/player/stats/day", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + PlayerPrefs.GetString("AccessToken", "")
            );
            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.responseCode == 401 && !hasRefreshedSession)
                {
                    bool refreshFinished = false;
                    bool refreshSucceeded = false;
                    LoginManager.Instance.RefreshSession(success =>
                    {
                        refreshSucceeded = success;
                        refreshFinished = true;
                    });

                    while (!refreshFinished)
                        yield return null;

                    if (refreshSucceeded)
                    {
                        isSaving = false;
                        yield return SaveWorkDay(onComplete, true);
                        yield break;
                    }
                }

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
                GameLogger.Error(
                    "Economy",
                    "อ่านผลการบันทึกไม่สำเร็จ"
                );

                isSaving = false;
                onComplete?.Invoke(false);
                yield break;
            }

            // อัปเดตยอดที่บันทึกสำเร็จแล้ว
            SavedCoins = result.coin;
            WorkDays = result.work_days;
            CustomersServed = result.total_customers_served;

            // Show today's totals before resetting the daily counters.
            ShowShiftSummary();

            // Reset daily earnings every day. Rent and ticket are only
            // deducted on days where isShiftEnd is true.
            OrderCoins = 0;
            TipCoins = 0;
            PenaltyCoins = 0;
            TicketExpense = 0;
            RentExpense = 0;
            pendingExpenseWorkDay = -1;

            // รีเซ็ตเฉพาะจำนวนลูกค้าของวันปัจจุบัน
            TodayCustomersServed = 0;
            TodayOrderCoins = 0;
            TodayTipCoins = 0;
            TodayPenaltyCoins = 0;

            UpdateNavbar();

            GameLogger.Info(
                "Economy",
                $"บันทึกสำเร็จ | วัน={WorkDays} | " +
                $"เหรียญ={SavedCoins} | ลูกค้า={CustomersServed} | " +
                $"จบกะ={isShiftEnd}"
            );
        }

        isSaving = false;
        onComplete?.Invoke(true);
    }

    // แสดงสรุปกะเพียงครั้งเดียว
    private void ShowShiftSummary()
    {
        if (shiftEnd != null)
        {
            shiftEnd.ShowSummary();
        }
        else
        {
            GameLogger.Warn(
                "CoinManager",
                "ยังไม่ได้กำหนด ShiftEnd ใน Inspector"
            );
        }
    }

    // อัปเดต Navbar จากระบบอื่นได้
    public void RefreshNavbar()
    {
        UpdateNavbar();

        GameLogger.Info(
            "Economy",
            $"RefreshNavbar | Coins={TotalCoins} | " +
            $"Days={WorkDays} | " +
            $"Customers={CustomersServed + TodayCustomersServed}"
        );
    }

    private void UpdateNavbar()
    {
        if (totalCoinsText != null)
        {
            totalCoinsText.text = TotalCoins.ToString();
        }

        // Order และ Tips แสดงยอดสะสมตลอดกะ
        if (orderCoinsText != null)
        {
            orderCoinsText.text = "Order " + OrderCoins;
        }

        if (tipCoinsText != null)
        {
            tipCoinsText.text = "Tips " + TipCoins;
        }

        if (totalcustomerservedText != null)
        {
            int totalServed =
                CustomersServed + TodayCustomersServed;

            totalcustomerservedText.text =
                "Served " + totalServed;
        }

        if (daysText != null)
        {
            daysText.text = WorkDays.ToString();
        }
    }

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

    [Serializable]
    private class FinishWorkDayRequest
    {
        public string user_id;
        public int coin;
        public int work_days;
        public int total_customers_served;
    }

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
