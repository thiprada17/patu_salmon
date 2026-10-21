
using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class CoffeeMachine : MonoBehaviour
{
    private Portfilter currentPortfilter;
    private Cup currentCup;

    private Vector3 originalPosition;
    private Vector3 portfilterOriginalPosition;
    private Vector3 cupOriginalPosition;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Coffee Settings")]
    [SerializeField] private float coffeeDuration = 2f;
    [SerializeField] private float shakeMagnitude = 0.02f;

    private bool isMakingCoffee = false;
    private Coroutine coffeeCoroutine;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void AttachPortfilter(Portfilter portfilter)
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (portfilter == null)
            return;

        currentPortfilter = portfilter;
        portfilterOriginalPosition = portfilter.transform.localPosition;

        GameLogger.DebugLog(
            "CoffeeMach",
            "Portfilter attached to Coffee Machine"
        );
    }

    public void RemovePortfilter(Portfilter portfilter)
    {
        if (currentPortfilter == portfilter)
        {
            currentPortfilter = null;

            GameLogger.DebugLog(
                "CoffeeMach",
                "Portfilter removed from Coffee Machine"
            );
        }
    }

    public void AttachCup(Cup cup)
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (cup == null)
            return;

        currentCup = cup;
        cupOriginalPosition = cup.transform.localPosition;

        GameLogger.DebugLog(
            "CoffeeMach",
            "Cup attached to Coffee Machine"
        );
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;

            GameLogger.DebugLog(
                "CoffeeMach",
                "Cup removed from Coffee Machine"
            );
        }
    }

    private void OnMouseDown()
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (isMakingCoffee)
        {
            GameLogger.DebugLog(
                "CoffeeMach",
                "Coffee Machine is working..."
            );
            return;
        }

        if (currentPortfilter == null)
        {
            GameLogger.Warn(
                "CoffeeMach",
                "ยังไม่ได้ใส่ Portfilter"
            );
            return;
        }

        if (!currentPortfilter.hasCoffeeGround)
        {
            GameLogger.DebugLog(
                "CoffeeMach",
                "Need coffee ground"
            );
            return;
        }

        if (currentCup == null)
        {
            GameLogger.Warn(
                "CoffeeMach",
                "ยังไม่ได้ใส่ Cup"
            );
            return;
        }

        if (currentCup.IsFull)
            return;

        if (audioSource != null && audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip);

        coffeeCoroutine = StartCoroutine(MakeCoffeeProcess());
    }

    private IEnumerator MakeCoffeeProcess()
    {
        isMakingCoffee = true;

        Portfilter portfilter = currentPortfilter;
        Cup cup = currentCup;

        if (portfilter == null || cup == null)
        {
            FinishCoffeeProcess();
            yield break;
        }

        // Save positions before shaking begins.
        originalPosition = transform.localPosition;
        portfilterOriginalPosition = portfilter.transform.localPosition;
        cupOriginalPosition = cup.transform.localPosition;

        float elapsed = 0f;

        while (elapsed < coffeeDuration)
        {
            // Stop coffee production if the tutorial starts.
            if (GameplayInputLock.IsLocked)
            {
                RestorePositions(portfilter, cup);
                FinishCoffeeProcess();
                yield break;
            }

            // Stop safely if either item is destroyed.
            if (portfilter == null || cup == null)
                break;

            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = Random.Range(-shakeMagnitude, shakeMagnitude);

            Vector3 shakeOffset = new Vector3(x, y, 0f);

            transform.localPosition = originalPosition + shakeOffset;
            portfilter.transform.localPosition =
                portfilterOriginalPosition + shakeOffset;
            cup.transform.localPosition =
                cupOriginalPosition + shakeOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        RestorePositions(portfilter, cup);

        // Do not produce coffee if gameplay has become locked.
        if (!GameplayInputLock.IsLocked &&
            portfilter != null &&
            cup != null &&
            currentPortfilter == portfilter &&
            currentCup == cup)
        {
            bool success = cup.AddIngredient(IngredientType.Coffee);

            if (success)
            {
                GameLogger.DebugLog(
                    "CoffeeMach",
                    "Coffee added to Cup!"
                );

                portfilter.RemoveCoffeeGround();
            }
            else
            {
                GameLogger.DebugLog(
                    "CoffeeMach",
                    "ไม่สามารถใส่ Coffee ลง Cup ได้"
                );
            }
        }

        FinishCoffeeProcess();
    }

    private void RestorePositions(Portfilter portfilter, Cup cup)
    {
        transform.localPosition = originalPosition;

        if (portfilter != null)
        {
            portfilter.transform.localPosition =
                portfilterOriginalPosition;
        }

        if (cup != null)
        {
            cup.transform.localPosition = cupOriginalPosition;
        }
    }

    private void FinishCoffeeProcess()
    {
        isMakingCoffee = false;
        coffeeCoroutine = null;
    }

    private void OnDisable()
    {
        if (coffeeCoroutine != null)
        {
            StopCoroutine(coffeeCoroutine);
            coffeeCoroutine = null;
        }

        isMakingCoffee = false;

        if (currentPortfilter != null)
        {
            currentPortfilter.transform.localPosition =
                portfilterOriginalPosition;
        }

        if (currentCup != null)
        {
            currentCup.transform.localPosition =
                cupOriginalPosition;
        }

        transform.localPosition = originalPosition;
    }
}
