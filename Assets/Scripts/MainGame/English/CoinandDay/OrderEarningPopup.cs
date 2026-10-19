
//using System.Collections;
//using TMPro;
//using UnityEngine;

//public class OrderEarningPopup : MonoBehaviour
//{
//    [SerializeField] private GameObject panel;
//    [SerializeField] private TMP_Text priceText;
//    [SerializeField] private TMP_Text tipText;
//    [SerializeField] private TMP_Text penaltyText;
//    [SerializeField] private float displayDuration = 3f;

//    private Coroutine hideCoroutine;

//    private void Awake()
//    {
//        if (panel != null)
//            panel.SetActive(false);
//    }

//    public void Show(int price, int tip, int penalty)
//    {
//        int net = price + tip - penalty;

//        if (priceText != null)
//            priceText.text = $"Price: {price}";

//        if (tipText != null)
//            tipText.text = $"Tip: {tip}";

//        if (penaltyText != null)
//            penaltyText.text = $"Pen: -{penalty}";


//        if (panel != null)
//            panel.SetActive(false);

//        if (hideCoroutine != null)
//            StopCoroutine(hideCoroutine);

//        hideCoroutine = StartCoroutine(HideAfterDelay());
//    }

//    private IEnumerator HideAfterDelay()
//    {
//        yield return new WaitForSeconds(displayDuration);

//        if (panel != null)
//            panel.SetActive(false);

//        hideCoroutine = null;
//    }
//}
