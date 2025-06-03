using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource bgmSource;
    public AudioSource effectSource;
    public AudioSource fryerSource;

    [Header("BGM")]
    public AudioClip titleBGM;
    public AudioClip gameBGM;
    public AudioClip fryerClip;

    [Header("Effect")]
    public AudioClip buttonClick;
    public AudioClip buttonHover;
    public AudioClip getItem;
    public AudioClip fryerFinish;

    [Range(0f, 1f)]
    public float masterVolume = 1f;

    private void Start()
    {
        PlayTitleBGM();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환에도 유지
        }
        else
        {
            Destroy(gameObject);  // 중복 제거
            return;
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        bgmSource.volume = volume;
        effectSource.volume = volume;
        // fryerSource는 무시
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume * masterVolume;
    }

    public void SetEffectVolume(float volume)
    {
        effectSource.volume = volume * masterVolume;
    }

    public void PlayTitleBGM()
    {
        bgmSource.clip = titleBGM;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayGameBGM()
    {
        bgmSource.clip = gameBGM;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }


    public void PlayButton(AudioClip clip)
    {
        effectSource.PlayOneShot(clip);
    }

    public void PlayGetItem()
    {
        effectSource.PlayOneShot(getItem);
    }

    public void PlayFryer()
    {
        fryerSource.clip = fryerClip;
        fryerSource.Play();
    }
    public void StopFryer()
    {
       fryerSource.Stop(); // 이건 fryerSource만 멈춤
    }

    public void FryerFinish()
    {
        effectSource.PlayOneShot(fryerFinish);
    }

    public void ButtonClick()
    {
        effectSource.PlayOneShot(buttonClick);
    }

    public void ButtonHover()
    {
        effectSource.PlayOneShot(buttonHover);
    }
}
