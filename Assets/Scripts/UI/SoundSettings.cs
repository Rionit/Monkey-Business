using Ami.BroAudio;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle muteToggle;

    [Header("Settings")]
    [Range(0f, 1f)] public float defaultVolume = 1f;

    private const string VolumeKey = "MasterVolume";
    private const string MuteKey = "MasterMute";

    private float lastVolume = 1f;
    private bool isMuted = false;

    private void Awake()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, defaultVolume);
        isMuted = PlayerPrefs.GetInt(MuteKey, 0) == 1;

        lastVolume = savedVolume;

        volumeSlider.value = isMuted ? 0f : savedVolume;
        muteToggle.isOn = isMuted;

        ApplyVolume(isMuted ? 0f : savedVolume);

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
    }

    private void OnVolumeChanged(float value)
    {
        if (isMuted && value > 0f)
        {
            isMuted = false;
            muteToggle.SetIsOnWithoutNotify(false);
            PlayerPrefs.SetInt(MuteKey, 0);
        }

        lastVolume = value;
        ApplyVolume(value);
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void OnMuteToggleChanged(bool mute)
    {
        isMuted = mute;

        if (isMuted)
        {
            ApplyVolume(0f);
            volumeSlider.SetValueWithoutNotify(0f);
        }
        else
        {
            ApplyVolume(lastVolume);
            volumeSlider.SetValueWithoutNotify(lastVolume);
        }

        PlayerPrefs.SetInt(MuteKey, isMuted ? 1 : 0);
    }
    private void ApplyVolume(float volume)
    {
        // BroAudio master volume control
        BroAudio.SetVolume(BroAudioType.All, volume);
    }
}