using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 호떡을 만드는 구역을 관리하는 클래스
/// 플레이어가 이 구역에 들어와서 E키를 누르면 호떡을 만들 수 있음
/// </summary>
public class Hottuk : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조
    private bool isMaking = false;          // 현재 호떡을 만들고 있는지 여부
    private DishZone dishZone;              // 접시 관리 구역 참조

    [Header("제작 설정")]
    [SerializeField] private float makeTime = 5f;     // 호떡 제작 시간
    [SerializeField] private int requiredFlour = 1;   // 필요 밀가루 개수
    [SerializeField] private int requiredSugar = 1;   // 필요 설탕 개수

    public Slider cookSlider;           // 연결된 슬라이더

    private void Start()
    {
        cookSlider.gameObject.SetActive(false);
        dishZone = FindFirstObjectByType<DishZone>();
    }

    /// <summary>
    /// 플레이어가 구역에 들어왔을 때 호출
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.currentZone = this;
            Debug.Log("호떡 제작 구역에 들어왔습니다. E키를 눌러 호떡을 만드세요.");
        }
    }

    /// <summary>
    /// 플레이어가 구역을 나갔을 때 호출
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            
            // 이 존이 현재 존이었다면 null로 설정
            if (player != null && player.currentZone == this)
            {
                player.currentZone = null;
                Debug.Log("호떡 제작 구역을 나갔습니다.");
            }
        }
    }

    /// <summary>
    /// 매 프레임마다 호출되는 업데이트 함수
    /// E키 입력 시 호떡 제작 시작
    /// </summary>
    private void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E) && !isMaking)
        {
            // 재료 확인 후 요리 시작
            if (player.flourCount >= requiredFlour && player.sugarCount >= requiredSugar)
            {
                if(!player.TryStartCooking()) return;
                StartCoroutine(CookProcess());
                TryMakeHottuk();
            }
            else
            {
                Debug.Log("재료가 부족합니다! (필요: 밀가루 1개, 설탕 1개)");
            }
        }
    }

    /// <summary>
    /// 호떡 제작 시도
    /// </summary>
    private void TryMakeHottuk()
    {
        // 재료 소모
        player.flourCount -= requiredFlour;
        player.sugarCount -= requiredSugar;

        // 제작 시작
        StartCoroutine(MakeHottukCoroutine());
    }

    /// <summary>
    /// 호떡 제작 코루틴
    /// </summary>
    private IEnumerator MakeHottukCoroutine()
    {
        isMaking = true;
        Debug.Log("호떡 제작 시작...");

        yield return new WaitForSeconds(makeTime);

        // 호떡 생성
        player.hottukCount++;
        player.HoldItem("hottuk");
        dishZone.AddDish();  // 접시 추가
        Debug.Log($"호떡 제작 완료! (현재 보유: {player.hottukCount}개)");

        // 상태 초기화
        isMaking = false;
        player.EndCooking(); 
        player.currentZone = null;
    }

    IEnumerator CookProcess()
    {
        isMaking = true;
        cookSlider.gameObject.SetActive(true); // 게이지 보이기
        cookSlider.value = 0f;

        float timer = 0f;
        while (timer < makeTime)
        {
            timer += Time.deltaTime;
            cookSlider.value = timer / makeTime;
            yield return null;
        }

        cookSlider.gameObject.SetActive(false); // 완료 후 숨기기
        Debug.Log("요리 완료!");
        isMaking = false;
        player.EndCooking();  // 요리 완료 시 EndCooking 호출
    }
}
