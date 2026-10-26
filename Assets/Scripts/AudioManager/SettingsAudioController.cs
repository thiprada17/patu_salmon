using UnityEngine;
using UnityEngine.UI;

public class SettingsAudioController : MonoBehaviour
{
    [Header("Settings Sliders")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    private AudioManager audioManager;

    private void OnEnable()
    {
        RefreshSliders();
    }

    private void Start()
    {
        RefreshSliders();
    }

    private void RefreshSliders()
    {
        audioManager = AudioManager.Instance;

        if (audioManager == null)
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

        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;
        bgmSlider.minValue = 0f;
        bgmSlider.maxValue = 1f;

        // Match the handles to the actual saved volume values.
        sfxSlider.SetValueWithoutNotify(audioManager.GetSFXVolume());
        bgmSlider.SetValueWithoutNotify(audioManager.GetBGMVolume());

        // Prevent duplicate callbacks.
        sfxSlider.onValueChanged.RemoveListener(audioManager.SetSFXVolume);
        bgmSlider.onValueChanged.RemoveListener(audioManager.SetBGMVolume);

        sfxSlider.onValueChanged.AddListener(audioManager.SetSFXVolume);
        bgmSlider.onValueChanged.AddListener(audioManager.SetBGMVolume);
    }

    private void OnDisable()
    {
        if (audioManager == null)
            return;

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(audioManager.SetSFXVolume);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(audioManager.SetBGMVolume);
    }
}