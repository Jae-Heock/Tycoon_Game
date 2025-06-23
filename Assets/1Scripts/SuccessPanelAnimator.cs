using UnityEngine;
using System.Collections;

/// <summary>
/// 성공 패널의 꽃 애니메이션을 제어하는 스크립트입니다.
/// </summary>
public class SuccessPanelAnimator : MonoBehaviour
{
    [Header("꽃 오브젝트")]
    public GameObject[] blackFlowers; // Black1, Black2, Black3를 연결할 배열
    public GameObject[] colorFlowers; // Flower1, Flower2, Flower3을 연결할 배열

    [Header("최종 이미지")]
    public GameObject finalImage; // 마지막에 나타날 이미지 오브젝트

    [Header("애니메이션 설정")]
    public float startDelay = 0.5f;        // 애니메이션 시작 전 대기 시간
    public float flowerAppearDelay = 0.5f; // 꽃이 하나씩 나타나는 간격
    public float finalImageDelay = 0.5f;   // 꽃이 모두 나타난 후 최종 이미지가 나오기까지의 대기 시간
    public float finalImageAnimDuration = 0.2f; // 최종 이미지가 내려오는 애니메이션 시간

    private Vector2 finalImageOriginalPosition; // 최종 이미지의 원래 위치를 저장할 변수

    void Awake()
    {
        // 최종 이미지의 원래 위치를 미리 저장해둡니다.
        if (finalImage != null)
        {
            // RectTransform이 있는 UI 오브젝트라고 가정합니다.
            finalImageOriginalPosition = finalImage.GetComponent<RectTransform>().anchoredPosition;
        }
    }

    /// <summary>
    /// 이 패널 오브젝트가 활성화될 때마다 애니메이션을 다시 시작합니다.
    /// </summary>
    void OnEnable()
    {
        StartAnimation();
    }

    private void StartAnimation()
    {
        // 1. 초기 상태 설정: 검은 꽃은 모두 켜고, 컬러 꽃은 모두 끕니다.
        foreach (var flower in blackFlowers)
        {
            if (flower != null) flower.SetActive(true);
        }
        foreach (var flower in colorFlowers)
        {
            if (flower != null) flower.SetActive(false);
        }

        // 최종 이미지도 처음에 비활성화합니다.
        if (finalImage != null)
        {
            finalImage.SetActive(false);
        }

        // 2. 애니메이션 코루틴을 시작합니다.
        StartCoroutine(AnimateFlowers());
    }

    private IEnumerator AnimateFlowers()
    {
        // 3. 애니메이션 시작 전 잠시 대기합니다.
        yield return new WaitForSeconds(startDelay);

        // 4. 컬러 꽃들을 순서대로 하나씩 활성화시킵니다.
        foreach (var flower in colorFlowers)
        {
            if (flower != null)
            {
                flower.SetActive(true);
                // 꽃이 나타날 때마다 성공 사운드를 재생합니다.
                if (SoundManager.instance != null)
                {
                    SoundManager.instance.PlaySuccess();
                }
            }
            yield return new WaitForSeconds(flowerAppearDelay);
        }

        // 5. 모든 꽃이 나타난 후, 최종 이미지 애니메이션을 준비합니다.
        yield return new WaitForSeconds(finalImageDelay);

        if (finalImage != null)
        {
            // 최종 등장 사운드를 재생합니다.
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlaySuccess2();
            }
            // 최종 이미지를 활성화하고 애니메이션 코루틴을 실행합니다.
            finalImage.SetActive(true);
            StartCoroutine(AnimateFinalImage());
        }
    }

    /// <summary>
    /// 최종 이미지를 위에서 아래로 내려오게 하는 애니메이션 코루틴입니다.
    /// </summary>
    private IEnumerator AnimateFinalImage()
    {
        RectTransform rect = finalImage.GetComponent<RectTransform>();
        
        // 시작 위치를 화면 위쪽 (원래 위치 + 500)으로 설정합니다.
        Vector2 startPos = finalImageOriginalPosition + new Vector2(0, 500f);
        Vector2 endPos = finalImageOriginalPosition;

        rect.anchoredPosition = startPos;

        float elapsedTime = 0f;
        while (elapsedTime < finalImageAnimDuration)
        {
            // Lerp를 사용하여 부드럽게 이동시킵니다.
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / finalImageAnimDuration);
            elapsedTime += Time.unscaledDeltaTime; // Time.timeScale에 영향을 받지 않도록 unscaledDeltaTime 사용
            yield return null;
        }

        rect.anchoredPosition = endPos; // 정확한 최종 위치에 고정
    }
} 