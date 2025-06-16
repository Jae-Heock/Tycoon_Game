using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UIElements;

/// <summary>
/// 주문 목록 패널을 열고 닫는 기능을 담당하는 클래스
/// </summary>
public class OrderListToggle : MonoBehaviour
{
    public RectTransform backGroundPanel;   // 주문 목록의 배경 패널(RectTransform)
    public GameObject openButton;           // 열기 버튼 오브젝트
    public float animDuration = 0.5f;       // 애니메이션 재생 시간
    private float originalHeight;           // 패널의 원래 높이 (열렸을 때)

    public RectTransform backGround567;     // 닫힌 상태에서 표시되는 요소
    public RectTransform backGround;

    void Start()
    {
        originalHeight = backGroundPanel.sizeDelta.y; // 현재 높이를 저장 (원래 상태)

        // ✅ 시작할 때 패널을 펼쳐진 상태로 유지
        backGroundPanel.sizeDelta = new Vector2(backGroundPanel.sizeDelta.x, originalHeight); 
        backGroundPanel.gameObject.SetActive(true);

        // 닫힌 패널 표시용 오브젝트는 비활성화
        backGround567.gameObject.SetActive(false);

        // 열기 버튼은 숨겨둠 (이미 열려있으므로)
        openButton.SetActive(false);
    }

    /// <summary>
    /// 열기 버튼 클릭 시 실행되는 함수
    /// 패널을 펼치고 애니메이션 실행
    /// </summary>
    public void OnOpenButtonClick()
    {
        openButton.SetActive(false);                   // 열기 버튼 숨기기
        backGroundPanel.gameObject.SetActive(true);    // 패널 활성화
        backGround567.gameObject.SetActive(false);     // 닫힌 패널 숨기기
        backGround.gameObject.SetActive(true);
        // 접힌 상태 → 펼쳐진 상태로 애니메이션 실행
        StartCoroutine(AnimatePanel(0f, originalHeight, animDuration, true));
    }

    /// <summary>
    /// 닫기 버튼 클릭 시 실행되는 함수
    /// 패널을 접고 닫힌 상태 오브젝트를 보여줌
    /// </summary>
    public void OnCloseButtonClick()
    {
        backGround567.gameObject.SetActive(true); // 닫힌 상태 표시용 오브젝트 활성화
        backGround.gameObject.SetActive(false);
        // 펼쳐진 상태 → 접힌 상태로 애니메이션 실행
        StartCoroutine(AnimatePanel(originalHeight, 0f, animDuration, false));
    }

    /// <summary>
    /// 패널의 높이를 서서히 변화시키는 애니메이션 코루틴
    /// </summary>
    IEnumerator AnimatePanel(float from, float to, float duration, bool opening)
    {
        float t = 0f;

        // 지정된 시간 동안 높이를 보간
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float h = Mathf.Lerp(from, to, t / duration);
            Vector2 size = backGroundPanel.sizeDelta;
            size.y = h;
            backGroundPanel.sizeDelta = size;
            yield return null;
        }

        // 마지막 정확한 값 적용
        Vector2 finalSize = backGroundPanel.sizeDelta;
        finalSize.y = to;
        backGroundPanel.sizeDelta = finalSize;

        // 닫기 후 상태 처리
        if (!opening)
        {
            backGroundPanel.gameObject.SetActive(false); // 패널 비활성화
            openButton.SetActive(true);                  // 열기 버튼 다시 표시
        }
    }
}
