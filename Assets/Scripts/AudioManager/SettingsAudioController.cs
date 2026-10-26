
using UnityEngine;
using UnityEngine.UI;

public class SettingsAudioController : MonoBehaviour
{
    [Header("Settings Sliders")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError(
                "SettingsAudioController: AudioManager not found!"
            );
            return;
        }

        if (sfxSlider == null || bgmSlider == null)
        {
            Debug.LogError(
                "SettingsAudioController: Assign both sliders in the Inspector."
            );
            return;
        }

        // Configure slider ranges.
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;

        bgmSlider.minValue = 0f;
        bgmSlider.maxValue = 1f;

        // Display saved volume settings without triggering callbacks.
        sfxSlider.SetValueWithoutNotify(
            AudioManager.Instance.GetSFXVolume()
        );

        bgmSlider.SetValueWithoutNotify(
            AudioManager.Instance.GetBGMVolume()
        );

        // Connect slider changes to the AudioManager.
        sfxSlider.onValueChanged.AddListener(
            AudioManager.Instance.SetSFXVolume
        );

        bgmSlider.onValueChanged.AddListener(
            AudioManager.Instance.SetBGMVolume
        );
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance == null)
            return;

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(
                AudioManager.Instance.SetSFXVolume
            );
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(
                AudioManager.Instance.SetBGMVolume
            );
        }
    }
}
