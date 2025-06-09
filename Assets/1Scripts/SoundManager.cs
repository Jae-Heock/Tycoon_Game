using System.Diagnostics;
using UnityEngine;
using System.Collections;

/// <summary>
/// 게임 전반의 사운드를 관리하는 매니저 클래스 (싱글턴 방식)
/// BGM, 효과음, 조리음 등 개별 오디오 소스를 통해 관리
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance; // 싱글턴 인스턴스

    public AudioSource bgmSource;    // 배경음(BGM) 재생용
    public AudioSource effectSource; // 일반 효과음 재생용
    public AudioSource dalgonaSource; // 달고나 소리용
    public AudioSource hottukSource;  // 호떡 소리용
    public AudioSource fryerSource;   // 프라이어 소리용
    public AudioSource boungSource;   // 붕어빵 소리용
    public AudioSource cleanSource;   // 설거지 소리용

    [Header("BGM")]
    public AudioClip titleBGM;       // 타이틀 화면 BGM
    public AudioClip gameBGM;        // 게임 화면 BGM

    [Header("Effect")]
    public AudioClip buttonClick;    // 버튼 클릭 효과음 -
    public AudioClip buttonHover;    // 버튼 호버 효과음 - 타이틀씬
    public AudioClip getItem;        // 아이템 획득 효과음

    public AudioClip fryer;         // 프라이어 소리
    public AudioClip boungIng;      // 붕어빵 만들때 나는소리 - 2초만 
    public AudioClip boungEnd;     // 붕어빵 다 만들었을때 나는소리 
    public AudioClip dalgona;      // 달고나 소리
    public AudioClip hottuk;      // 호떡 소리

    public AudioClip clean;         // 설거지소리

    public AudioClip badSound1;      // 나쁜손님 소리1
    public AudioClip badSound2;      // 나쁜손님 소리2
    public AudioClip badSound3;      // 나쁜손님 소리3
    public AudioClip startHororagi; // 게임씬 시작 호루라기 
    public AudioClip badCustomBackGround; // 나쁜손님 배경음

    public AudioClip success; // 성공 소리 


    [Range(0f, 1f)]
    public float masterVolume = 1f;  // 마스터 볼륨 (0~1)

    // 게임이 시작될 때 타이틀 BGM 재생
    private void Start()
    {
        PlayTitleBGM();
    }

    // 싱글턴 초기화 및 중복 제거
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환에도 유지되도록 설정
        }
        else
        {
            Destroy(gameObject);  // 중복 객체 제거
            return;
        }
    }

    /// <summary>
    /// 마스터 볼륨을 설정하고 BGM, 효과음 볼륨에 반영
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        bgmSource.volume = volume;
        effectSource.volume = volume;
        // fryerSource는 개별 제어되므로 여기선 제외
    }

    /// <summary>
    /// BGM 볼륨 설정 (마스터 볼륨 반영)
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume * masterVolume;
    }

    /// <summary>
    /// 효과음 볼륨 설정 (마스터 볼륨 반영)
    /// </summary>
    public void SetEffectVolume(float volume)
    {
        effectSource.volume = volume * masterVolume;
    }

    /// <summary>
    /// 타이틀 화면용 BGM 재생
    /// </summary>
    public void PlayTitleBGM()
    {
        bgmSource.clip = titleBGM;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    /// <summary>
    /// 게임 플레이 중 BGM 재생
    /// </summary>
    public void PlayGameBGM()
    {
        bgmSource.clip = gameBGM;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    /// <summary>
    /// BGM 완전 정지
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// 지정된 효과음 클립을 재생 (버튼 클릭 등)
    /// </summary>
    public void PlayButton(AudioClip clip)
    {
        effectSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 아이템 획득 효과음 재생
    /// </summary>
    public void PlayGetItem()
    {
        effectSource.PlayOneShot(getItem);
    }

    /// <summary>
    /// 프라이어(Fryer) 작동음 재생 (루프)
    /// </summary>
    public void PlayFryer()
    {
        fryerSource.PlayOneShot(fryer, 0.7f);
        StartCoroutine(StopFryer());
    }

    public IEnumerator StopFryer()
    {
        yield return new WaitForSeconds(5f);
        fryerSource.Stop();
    }

    /// <summary>
    /// 버튼 클릭 효과음 재생
    /// </summary>
    public void ButtonClick()
    {
        effectSource.PlayOneShot(buttonClick);
    }

    /// <summary>
    /// 버튼 호버(마우스 오버) 효과음 재생
    /// </summary>
    public void ButtonHover()
    {
        effectSource.PlayOneShot(buttonHover);
    }

    /// <summary>
    ///  나쁜손님 등장씬 소리 재생 
    /// </summary>
    public void PlayBadSound1()
    {
        effectSource.PlayOneShot(badSound1, 1.5f);
    }

    public void PlayBadSound2()
    {
        effectSource.PlayOneShot(badSound2, 1.5f);
    }

    public void PlayBadSound3()
    {
        effectSource.PlayOneShot(badSound3, 1.5f);
    }

    // 게임씬 시작 호루라기 재생
    public void PlayStartHororagi()
    {
        effectSource.PlayOneShot(startHororagi);
    }

    // 나쁜손님 배경음 재생
    public void PlayBadCustomBackGround()
    {
        effectSource.PlayOneShot(badCustomBackGround, 0.7f);
        StartCoroutine(StopBadCustomBackGroundAfterDelay(3f));
    }

    private IEnumerator StopBadCustomBackGroundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        effectSource.Stop();
    }

    // 붕어빵 소리
    public void PlayBoungIng()
    {
        boungSource.PlayOneShot(boungIng, 0.7f);
        StartCoroutine(StopBoungIng());
    }

    public IEnumerator StopBoungIng()
    {
        yield return new WaitForSeconds(3f);
        boungSource.Stop();
    }

    // 붕어빵 다 만들었을때 소리
    public void PlayBoungEnd()
    {
        effectSource.PlayOneShot(boungEnd);
    }

    // 설거지 소리
    public void PlayClean()
    {
        cleanSource.PlayOneShot(clean, 0.7f);
        StartCoroutine(StopClean());
    }

    public IEnumerator StopClean()
    {
        yield return new WaitForSeconds(5f);
        cleanSource.Stop();
    }

    // 달고나 소리
    public void PlayDalgona()
    {
        dalgonaSource.PlayOneShot(dalgona, 0.7f);
        StartCoroutine(StopDalgona());
    }

    public IEnumerator StopDalgona()
    {
        yield return new WaitForSeconds(4f);
        dalgonaSource.Stop();
    }

    // 호떡 소리
    public void PlayHottuk()
    {
        hottukSource.PlayOneShot(hottuk, 0.7f);
        StartCoroutine(StopHottuk());
    }

    public IEnumerator StopHottuk()
    {
        yield return new WaitForSeconds(3f);
        hottukSource.Stop();
    }

    public void PlaySuccess()
    {
        effectSource.PlayOneShot(success);
    }

}
