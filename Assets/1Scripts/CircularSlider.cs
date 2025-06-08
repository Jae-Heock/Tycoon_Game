using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Video;

public class CircularSlider : MonoBehaviour
{
    [Header("🔘 설정")]
    public Image fillImage;              // Radial 타입 이미지
    public float fillSpeed = 0.5f;       // 슬라이더 채워지는 속도
    public GameObject Ui;
    public string nextSceneName = "GameScene"; // 이동할 씬 이름
    public VideoPlayer videoPlayer; // Inspector에서 할당

    [Range(0f, 1f)] private float value = 0f;
    private bool isFilling = false;
    private bool isLoading = false;


    void Start()
    {
        value = 0f;
        Ui.SetActive(true);

        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (!isLoading)
        {
            isLoading = true;
            StartCoroutine(LoadSceneCleanly());
        }
    }

    void Update()
    {
        if (isLoading) return; // 씬 전환 중엔 무시

        // 키 입력 체크
        if (Input.GetKey(KeyCode.Space))
        {
            isFilling = true;
        }
        else
        {
            isFilling = false;
        }

        if(value == 0f)
            Ui.SetActive(true);
        else
            Ui.SetActive(false);
           

        // 슬라이더 값 변화
        if (isFilling)
            value = Mathf.MoveTowards(value, 1f, fillSpeed * Time.deltaTime);
        else
            value = Mathf.MoveTowards(value, 0f, fillSpeed * Time.deltaTime); // 천천히 감소

        // 이미지 채우기 반영
        if (fillImage != null)
            fillImage.fillAmount = value;

        // 씬 전환 조건
        if (value >= 1f && !string.IsNullOrEmpty(nextSceneName))
        {
            isLoading = true;
            StartCoroutine(LoadSceneCleanly());
        }
    }

    public void SetValue(float newValue)
    {
        value = Mathf.Clamp01(newValue);
        if (fillImage != null)
            fillImage.fillAmount = value;
    }

    private IEnumerator LoadSceneCleanly()
    {
        // 현재 카메라의 렌더 타겟 해제 (영상 정리)
        if (Camera.main != null)
            Camera.main.targetTexture = null;

        yield return null;

        // 사용하지 않는 리소스 정리
        Resources.UnloadUnusedAssets();

        // 씬 비동기 로딩
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);

        // 로딩 완료까지 대기
        while (!asyncLoad.isDone)
            yield return null;
    }
}
