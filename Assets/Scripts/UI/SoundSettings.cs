using Ami.BroAudio;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private Toggle ncsToggle;

    [Header("Settings")]
    [Range(0f, 1f)] public float defaultVolume = 1f;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string MuteKey = "MasterMute";
    private const string NCSKey = "UseNoCopyright";

    private float lastMusicVolume = 1f;
    private float lastSfxVolume = 1f;
    private bool isMuted = false;

    private void Awake()
    {
        float savedMusic = PlayerPrefs.GetFloat(MusicVolumeKey, defaultVolume);
        float savedSfx = PlayerPrefs.GetFloat(SfxVolumeKey, defaultVolume);

        ncsToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(NCSKey, 1) == 1);
        isMuted = PlayerPrefs.GetInt(MuteKey, 0) == 1;

        lastMusicVolume = savedMusic;
        lastSfxVolume = savedSfx;

        musicSlider.value = isMuted ? 0f : savedMusic;
        sfxSlider.value = isMuted ? 0f : savedSfx;
        muteToggle.SetIsOnWithoutNotify(isMuted);

        ApplyVolume(isMuted ? 0f : savedMusic, isMuted ? 0f : savedSfx);

        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        ncsToggle.onValueChanged.AddListener(OnNCSToggleChanged);
        muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
    }

    private void OnMusicChanged(float value)
    {
        if (isMuted && value > 0f)
        {
            isMuted = false;
            muteToggle.SetIsOnWithoutNotify(false);
            PlayerPrefs.SetInt(MuteKey, 0);
        }

        lastMusicVolume = value;
        ApplyVolume(musicSlider.value, sfxSlider.value);
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
    }

    private void OnSfxChanged(float value)
    {
        if (isMuted && value > 0f)
        {
            isMuted = false;
            muteToggle.SetIsOnWithoutNotify(false);
            PlayerPrefs.SetInt(MuteKey, 0);
        }

        lastSfxVolume = value;
        ApplyVolume(musicSlider.value, sfxSlider.value);
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
    }

    private void OnMuteToggleChanged(bool mute)
    {
        isMuted = mute;

        if (isMuted)
        {
            ApplyVolume(0f, 0f);
            musicSlider.SetValueWithoutNotify(0f);
            sfxSlider.SetValueWithoutNotify(0f);
        }
        else
        {
            musicSlider.SetValueWithoutNotify(lastMusicVolume);
            sfxSlider.SetValueWithoutNotify(lastSfxVolume);
            ApplyVolume(lastMusicVolume, lastSfxVolume);
        }

        PlayerPrefs.SetInt(MuteKey, isMuted ? 1 : 0);
    }
    
    private void OnNCSToggleChanged(bool useNoCopyrightMusic)
    {
        PlayerPrefs.SetInt(NCSKey, useNoCopyrightMusic ?  1 : 0);
    }

    private void ApplyVolume(float musicVolume, float sfxVolume)
    {
        BroAudio.SetVolume(BroAudioType.Music, musicVolume);
        BroAudio.SetVolume(BroAudioType.SFX, sfxVolume);
        BroAudio.SetVolume(BroAudioType.Ambience, sfxVolume);
        BroAudio.SetVolume(BroAudioType.UI, sfxVolume);
    }
}