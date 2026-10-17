using TMPro;
using UnityEngine;

public class ShiftEnd : MonoBehaviour
{
    [Header("Summary Text")]
    [SerializeField] private TMP_Text daysText;
    [SerializeField] private TMP_Text totalCoinsText;
    [SerializeField] private TMP_Text tipsText;
    [SerializeField] private TMP_Text outticketText;
    [SerializeField] private TMP_Text outrentText;

    [SerializeField] private TMP_Text TotalEarnText;

    [SerializeField] private GameObject firerplatu;
    [SerializeField] private GameObject firersalmon;

    [SerializeField] private GameObject shiftSummaryPopup;

    public void ShowSummary()
    {
        if (shiftSummaryPopup != null)
        {
            shiftSummaryPopup.SetActive(true);
            Time.timeScale = 0f;
        }

        if (CoinManager.Instance == null)
        {
            GameLogger.Error("ShiftEnd", "ไม่พบ CoinManager");
            return;
        }

        CoinManager coin = CoinManager.Instance;

        string character = PlayerPrefs.GetString("SelectedCharacter", "salmon");

        firersalmon.SetActive(character == "salmon");
        firerplatu.SetActive(character != "salmon");

        daysText.text = coin.WorkDays.ToString();

        // แสดงรายได้จากออร์เดอร์และทิป
        totalCoinsText.text = coin.OrderCoins.ToString();
        tipsText.text = coin.TipCoins.ToString();

        // แสดงค่าใช้จ่าย
        outticketText.text = coin.TicketExpense.ToString();
        outrentText.text = coin.RentExpense.ToString();

        // รายได้สุทธิหลังหักค่าปรับและค่าใช้จ่าย
        TotalEarnText.text = coin.TotalShiftEarning.ToString();

        gameObject.SetActive(true);

        GameLogger.Info(
            "ShiftEnd",
            $"Days={coin.WorkDays}, " +
            $"Order={coin.OrderCoins}, Tips={coin.TipCoins}, " +
            $"Penalty={coin.PenaltyCoins}, " +
            $"Ticket={coin.TicketExpense}, Rent={coin.RentExpense}, " +
            $"Total={coin.TotalShiftEarning}"
        );
    }



    public void NextShift()
    {
       if (shiftSummaryPopup != null)
       {
            shiftSummaryPopup.SetActive(false);
       }
        gameObject.SetActive(false);
        shiftSummaryPopup.SetActive(false);
        Time.timeScale = 1f;
    }
}