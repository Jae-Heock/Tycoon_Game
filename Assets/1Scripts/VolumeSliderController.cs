using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    public enum VolumeType { Master, BGM, Effect }
    public VolumeType volumeType;

    public Slider slider;
    private SoundManager soundManager;

    private void Start()
    {
        soundManager = GameObject.FindFirstObjectByType<SoundManager>();

        if (slider == null)
            slider = GetComponent<Slider>();

        // 초기값 동기화
        float initialValue = 0.5f;

        switch (volumeType)
        {
            case VolumeType.Master:
                initialValue = soundManager.masterVolume;
                break;
            case VolumeType.BGM:
                initialValue = soundManager.bgmSource.volume;
                break;
            case VolumeType.Effect:
                initialValue = soundManager.effectSource.volume;
                break;
        }

        slider.value = initialValue;
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        switch (volumeType)
        {
            case VolumeType.Master:
                soundManager.SetMasterVolume(value);
                break;
            case VolumeType.BGM:
                soundManager.SetBGMVolume(value);
                break;
            case VolumeType.Effect:
                soundManager.SetEffectVolume(value);
                break;
        }
    }
}
