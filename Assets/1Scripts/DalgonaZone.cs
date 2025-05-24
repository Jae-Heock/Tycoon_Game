using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// 달고나를 만드는 구역을 관리하는 클래스
/// 플레이어가 이 구역에 들어와서 E키를 누르면 달고나를 만들 수 있음
/// </summary>
public class DalgonaZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조
    private bool isMaking = false;          // 현재 달고나를 만들고 있는지 여부
    private DishZone dishZone;              // 접시 관리 구역 참조
    public Slider cookSlider;           // 연결된 슬라이더

    [Header("제작 설정")]
    [SerializeField] public float makeTime = 5f;     // 기본 달고나 제작 시간
    [SerializeField] private int requiredSugar = 1;   // 필요 설탕 개수

    private void Start()
    {
        cookSlider.gameObject.SetActive(false);
        dishZone = FindFirstObjectByType<DishZone>();
    }

    /// <summary>
    /// 현재 적용될 제작 시간을 반환
    /// </summary>
    private float GetCurrentMakeTime()
    {
        // 스킬이 적용된 경우 플레이어의 cookTime 사용, 아니면 기본 makeTime 사용
        return player.cookTime > 0 ? player.cookTime : makeTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.currentZone = this;
            Debug.Log("달고나 제작 구역에 들어왔습니다. E키를 눌러 달고나를 만드세요.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            
            // 이 존이 현재 존이었다면 null로 설정
            if (player != null && player.currentZone == this)
            {
                player.currentZone = null;
                Debug.Log("달고나 제작 구역을 나갔습니다.");
            }
        }
    }

    private void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E) && !isMaking)
        {
            if (!string.IsNullOrEmpty(player.currentFood))
            {
                Debug.Log("이미 음식을 들고 있어 요리를 시작할 수 없습니다!");
                return;
            }
            // 재료 확인 후 요리 시작
            if (player.sugarCount >= requiredSugar)
            {
                if(!player.TryStartCooking()) return;
                StartCoroutine(CookProcess());
                TryMakeDalgona();
            }
            else
            {
                Debug.Log("재료가 부족합니다! (필요: 설탕 1개)");
            }
        }
    }

    private void TryMakeDalgona()
    {
        // 재료 소모
        player.sugarCount -= requiredSugar;

        // 제작 시작
        StartCoroutine(MakeDalgonaCoroutine());
    }

    private IEnumerator MakeDalgonaCoroutine()
    {
        isMaking = true;
        player.isMove = false;
        Debug.Log("달고나 제작 시작...");
        
        player.PlayDalgonaAnimation();
        yield return new WaitForSeconds(GetCurrentMakeTime());
        player.StopDalgonaAnimation();

        player.dalgonaCount++;
        player.HoldItem("dalgona");
        dishZone.AddDish();
        Debug.Log($"달고나 제작 완료! (현재 보유: {player.dalgonaCount}개)");

        isMaking = false;
        player.isMove = true;
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
