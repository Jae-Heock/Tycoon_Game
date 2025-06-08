using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoVolumeController : MonoBehaviour
{
    public Slider volumeSlider;          // 슬라이더 참조
    public VideoPlayer videoPlayer;      // 비디오 플레이어 참조
    void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
            SetVolume(volumeSlider.value); // 초기 볼륨 설정
        }
    }

    private void SetVolume(float value)
    {
        videoPlayer.SetDirectAudioVolume(0, value);
    }
}
